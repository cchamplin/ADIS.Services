using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ADIS.Core.ComponentServices;
using ADIS.Services.Responses;
using log4net;

namespace ADIS.Services
{
    public abstract class HttpAppHostBase : AppHostBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HttpAppHostBase));

        protected static int ThreadsPerCore = 16;

        public int PoolSize
        {
            get
            {
                return Environment.ProcessorCount * ThreadsPerCore;
            }
        }

        protected HttpListener listener;
        protected bool running;
        protected string reservedUrl;

        private readonly AutoResetEvent nextRequest = new AutoResetEvent(false);

        protected virtual Task AsyncProcessRequest(HttpListenerContext context)
        {
            if (string.IsNullOrEmpty(context.Request.RawUrl))
            {
                return ((object)null).AsTaskResult();
            }

            var container = ComponentServices.Fetch("Services");
            if (!container.ServiceTypeRegistered(typeof(IService)))
            {
                throw new Exception("A service host has not been initialized or registered");
            }

            var service = container.Resolve<IService>();
            
            var request = context.AsRequest();
            var response = request.Response;

            var handler = HttpHandlerFactory.GetHandler(request);
            var requestHandler = handler as IADISHttpHandler;
            if (requestHandler != null)
            {
                Task task = requestHandler.AsyncProcessRequest(request,response);
                task.ContinueWith(x => response.Close(), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.AttachedToParent);

                return task;
            }
            return new NotImplementedException("Cannot resolve route").AsTaskException();

        }

        public bool Running
        {
            get { return running; }
        }

        public bool Listening
        {
            get
            {
                return (listener != null && running && listener.IsListening);
            }
        }

        public override IService Start(string urlEndpoint)
        {
            if (Start(urlEndpoint, ListenCallback))
            {
                return this;
            }
            return null;
        }

        public virtual void Stop()
        {
            if (listener == null)
                return;

            try
            {
                this.listener.Close();
                
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode != 995)
                {
                    log.Error("Failed to close listener",ex);
                }

            }
            catch (Exception ex)
            {
                log.Error("Failed to close listener", ex);
            }
            finally
            {
                if (reservedUrl != null)
                {
                    RemoveUrlReservationFromAcl(reservedUrl);
                    reservedUrl = null;
                }
                this.running = false;
                this.listener = null;
            }

        }

        protected virtual bool Start(string urlEndpoint, WaitCallback callback)
        {
            if (this.running)
                return true;

            if (this.listener == null)
                listener = new HttpListener();

            listener.Prefixes.Add(urlEndpoint);


            try
            {
                listener.Start();
                running = true;
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 5 && reservedUrl == null)
                {
                    reservedUrl = AddUrlReservationToAcl(urlEndpoint);
                    if (reservedUrl == null)
                    {
                        log.Fatal("Failed to start listener", ex);
                        running = false;
                        return false;
                    }
                    listener = null;
                    running = false;
                    return Start(urlEndpoint, ListenCallback);
                }
            }
            catch (Exception ex)
            {
                log.Fatal("Failed to start listener", ex);
                running = false;
                return false;
            }
            ThreadPool.QueueUserWorkItem(callback);
            return true;
        }

        protected virtual void ListenCallback(object obj)
        {
            while (Listening)
            {
                try
                {
                    listener.BeginGetContext(GetContextCallback, listener);
                    nextRequest.WaitOne();
                }
                catch (Exception ex)
                {
                    log.Error("Listener failure", ex);
                }
            }
        }

        private void GetContextCallback(IAsyncResult result)
        {
            var listener = result.AsyncState as HttpListener;
            if (listener == null)
                return;

            HttpListenerContext context = null;

            if (!Listening)
            {
                return;
            }
            try
            {
                context = listener.EndGetContext(result);
                nextRequest.Set();
                if (context != null) {
                    try
                    {
                        Task task = AsyncProcessRequest(context);
                        task.ContinueWith(x => ErrorHandler(x.Exception, context), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.AttachedToParent);
                        if (task.Status == TaskStatus.Created)
                            task.RunSynchronously();
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler(ex, context);
                    }
                }
            }
            catch (Exception ex)
            {
                nextRequest.Set();
                log.Warn("Failed to end listener", ex);
                return;
            }
            

            
        }

        public static void ErrorHandler(Exception ex, HttpListenerContext context)
        {
            try
            {
                if (ex is AggregateException)
                {
                    if (ex.InnerException != null && ((AggregateException)ex).InnerExceptions.Count == 1)
                    {
                        ex = ((AggregateException)ex).InnerExceptions[0];
                    }
                }


                var httpReq = CreateHttpRequest(context);
                log.Error("Error Processing Request", ex);

                WriteUnhandledErrorResponse(httpReq, ex);
            }
            catch (Exception errorEx)
            {
                log.Error("Error Processing Request(Exception while writing error to the response)", errorEx);
            }
        }

        private static IHttpRequest CreateHttpRequest(HttpListenerContext context)
        {
            var httpReq = context.AsRequest();
            return httpReq;
        }

        public static void WriteUnhandledErrorResponse(IHttpRequest httpReq, Exception ex)
        {
            var errorResponse = new ErrorResponse(
            
                new ResponseStatus(httpReq.RawUrl,ex.Message)
               
           ,ex.StackTrace);

            var serializer = new FastSerialize.Serializer(typeof(FastSerialize.JsonSerializerGeneric));

            var httpRes = httpReq.Response;
            //var contentType = httpReq.ResponseContentType;

            //var serializer = HostContext.ContentTypes.GetResponseSerializer(contentType);
            //if (serializer == null)
            //{
            //    contentType = HostContext.Config.DefaultContentType;
            //    serializer = HostContext.ContentTypes.GetResponseSerializer(contentType);
            //}

            //var httpError = ex as IHttpError;
            //if (httpError != null)
            //{
           //     httpRes.StatusCode = httpError.Status;
            //    httpRes.StatusDescription = httpError.StatusDescription;
           // }
           // else
           // {
                httpRes.StatusCode = 500;
           // }

            httpRes.ContentType = "text/json";

            var responseData = serializer.Serialize(errorResponse,false,false);

            httpRes.Write(responseData);
        }

        /// <summary>
        /// Reserves the specified URL for non-administrator users and accounts. 
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/cc307223(v=vs.85).aspx
        /// </summary>
        /// <remarks>Borrowed from ServiceStack https://github.com/ServiceStack/ServiceStack/blob/master/src/ServiceStack/Host/HttpListener/HttpListenerBase.cs</remarks>
        /// <returns>Reserved Url if the process completes successfully</returns>
        public static string AddUrlReservationToAcl(string urlBase)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return null;

            try
            {
                string cmd, args;

                // use HttpCfg for windows versions before Version 6.0, else use NetSH
                if (Environment.OSVersion.Version.Major < 6)
                {
                    var sid = System.Security.Principal.WindowsIdentity.GetCurrent().User;
                    cmd = "httpcfg";
                    args = string.Format(@"set urlacl /u {0} /a D:(A;;GX;;;""{1}"")", urlBase, sid);
                }
                else
                {
                    cmd = "netsh";
                    args = string.Format(@"http add urlacl url={0} user={1}\{2} listen=yes", urlBase, Environment.UserDomainName, Environment.UserName);
                }

                var psi = new ProcessStartInfo(cmd, args)
                {
                    Verb = "runas",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                };

                Process.Start(psi).WaitForExit();

                return urlBase;
            }
            catch
            {
                return null;
            }
        }
        public static void RemoveUrlReservationFromAcl(string urlBase)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return;

            try
            {

                string cmd, args;

                if (Environment.OSVersion.Version.Major < 6)
                {
                    cmd = "httpcfg";
                    args = string.Format(@"delete urlacl /u {0}", urlBase);
                }
                else
                {
                    cmd = "netsh";
                    args = string.Format(@"http delete urlacl url={0}", urlBase);
                }

                var psi = new ProcessStartInfo(cmd, args)
                {
                    Verb = "runas",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                };

                Process.Start(psi).WaitForExit();
            }
            catch
            {
                /* ignore */
            }
        }

        private bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            base.Dispose();

            lock (this)
            {
                if (disposed) return;

                if (disposing)
                {
                    this.Stop();
                }

                //release unmanaged resources here...
                disposed = true;
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
