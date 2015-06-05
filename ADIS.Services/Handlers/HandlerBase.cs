using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using log4net;

namespace ADIS.Services.Handlers
{
    public abstract class HandlerBase : IHttpAsyncHandler, IADISHttpHandler
    {
        internal static readonly ILog log = LogManager.GetLogger(typeof(HandlerBase));
        internal static readonly Dictionary<byte[], byte[]> NetworkInterfaceIpv4Addresses = new Dictionary<byte[], byte[]>();
        internal static readonly byte[][] NetworkInterfaceIpv6Addresses = new byte[0][];

        static HandlerBase()
        {
            try
            {
                foreach (var addr in GetAllNetworkInterfaceIpv4Addresses())
                {
                    NetworkInterfaceIpv4Addresses[addr.Key.GetAddressBytes()] = addr.Value.GetAddressBytes();
                }



                NetworkInterfaceIpv6Addresses = GetAllNetworkInterfaceIpv6Addresses().ConvertAll(x => x.GetAddressBytes()).ToArray();
            }
            catch (Exception ex)
            {
                log.Warn("Failed to retrieve IP Addresses, some security restriction features may not work: " + ex.Message, ex);
            }
        }


        private static Dictionary<IPAddress, IPAddress> GetAllNetworkInterfaceIpv4Addresses()
        {
            var map = new Dictionary<IPAddress, IPAddress>();

            try
            {

                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    foreach (var uipi in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (uipi.Address.AddressFamily != AddressFamily.InterNetwork) continue;

                        if (uipi.IPv4Mask == null) continue; //ignore 127.0.0.1
                        map[uipi.Address] = uipi.IPv4Mask;
                    }
                }

            }
            catch /*(NotImplementedException ex)*/
            {
                //log.Warn("MONO does not support NetworkInterface.GetAllNetworkInterfaces(). Could not detect local ip subnets.", ex);
            }
            return map;
        }

        private static List<IPAddress> GetAllNetworkInterfaceIpv6Addresses()
        {
            var list = new List<IPAddress>();

            try
            {

                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    foreach (var uipi in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (uipi.Address.AddressFamily != AddressFamily.InterNetworkV6) continue;
                        list.Add(uipi.Address);
                    }
                }

            }
            catch /*(NotImplementedException ex)*/
            {
                //log.Warn("MONO does not support NetworkInterface.GetAllNetworkInterfaces(). Could not detect local ip subnets.", ex);
            }

            return list;
        }


        public static IPAddress GetIpAddress(System.ServiceModel.OperationContext context)
        {
#if !MONO
            var prop = context.IncomingMessageProperties;
            if (context.IncomingMessageProperties.ContainsKey(System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name))
            {
                var endpoint = prop[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name]
                    as System.ServiceModel.Channels.RemoteEndpointMessageProperty;
                if (endpoint != null)
                {
                    return IPAddress.Parse(endpoint.Address);
                }
            }
#endif
            return null;
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            if (cb == null)
            {
                throw new ArgumentNullException("Callback cannot be null");
            }

            Task task = AsyncProcessRequest(context.Request.RequestContext.HttpContext);

            task.ContinueWith(x => cb(x));

            if (task.Status == TaskStatus.Created)
            {
                task.Start();
            }
            return task;
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            Task task = result as Task;
            if (task == null)
            {
                throw new InvalidOperationException("Result must be a task");
            }
            task.Wait();
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public virtual Task AsyncProcessRequest(HttpContextBase context)
        {
            var request = new AspRequest(context);
            return AsyncProcessRequest(request, request.Response);
        }

        public virtual Task AsyncProcessRequest(IHttpRequest request, IResponse response)
        {
            var task = CreateRequestTask(request, response);
            task.Start();
            return task;
        }
        protected virtual Task CreateRequestTask(IHttpRequest request, IResponse response)
        {
            var culture = Thread.CurrentThread.CurrentCulture;
            var uiCulture = Thread.CurrentThread.CurrentUICulture;

            var context = HttpContext.Current;

            Action action = (() => {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = uiCulture;
                if (HttpContext.Current == null)
                    HttpContext.Current = context;
                ProcessRequest(request,response);
            });

            return new Task(action);
        }

        public virtual void ProcessRequest(IHttpRequest request, IResponse response)
        {
            throw new NotImplementedException();
        }

        //public abstract object CreateRequest(IRequest request);

       

        //public abstract object GetResponse(IRequest request);

        public virtual void ProcessRequest(HttpContext context)
        {
            Task task = AsyncProcessRequest(context.Request.RequestContext.HttpContext);
            if (task.Status == TaskStatus.Created)
            {
                task.RunSynchronously();
                return;
            }
            task.Wait();
        }
        public virtual void ProcessRequest(HttpContextBase context)
        {
            var request = new AspRequest(context);
            ProcessRequest(request, request.Response);
        }
        public virtual void ProcessRequest(HttpListenerContext context)
        {
            var request = new ListenerRequest(context);
            ProcessRequest(request, request.Response);
        }
    }
}
