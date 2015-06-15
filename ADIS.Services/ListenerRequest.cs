using ADIS.Core.ComponentServices.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ADIS.Services
{
    public class ListenerRequest : IHttpRequest
    {
        protected readonly HttpListenerRequest request;
        protected readonly IResponse response;
        protected Dictionary<string,IPostedFile> files;
        protected Dictionary<string, Cookie> cookies;
        protected Dictionary<string, string> form;
        protected string data;
        //protected Dictionary<string, object> items;
        public ListenerRequest(HttpListenerContext context)
        {
            this.request = context.Request;
            
            this.response = new ListenerResponse(context.Response);

                cookies = new Dictionary<string, Cookie>();
                for (var i = 0; i < this.request.Cookies.Count; i++)
                {
                    var httpCookie = this.request.Cookies[i];
                    cookies[httpCookie.Name] = httpCookie;
                }

               

        }

        public Dictionary<string,IPostedFile> Files
        {
            get
            {
                if (files == null)
                {
                    files = new Dictionary<string, IPostedFile>();
                    form = new Dictionary<string, string>();
                    if (request.ContentType != null)
                    {
                        if (request.ContentType.StartsWith("multipart/form-data"))
                        {
                            // form = new WebROCollection();
                            LoadMultiPart();
                            // form.Protect();
                        }
                    }
                }
                return files;
            }
        }

        private Stream GetSubStream(Stream stream)
        {


            if (stream is MemoryStream)
            {
                MemoryStream other = (MemoryStream)stream;
                return new MemoryStream(other.GetBuffer(), 0, (int)other.Length, false, true);
            }
            else
            {
                var memoryStream = new MemoryStream();
                var buffer = new byte[request.ContentLength64];
                

                int i = 0;
                do
                {
                    i = stream.Read(buffer, 0, (int)request.ContentLength64);
                    memoryStream.Write(buffer, 0, i);
                } while (i > 0);
                memoryStream.Flush();
                memoryStream.Position = 0;
                return memoryStream;
                
            }




            throw new NotSupportedException("The stream is " + stream.GetType());
        }
        void AddRawKeyValue(StringBuilder key, StringBuilder value)
        {
            string decodedKey = HttpUtility.UrlDecode(key.ToString(), request.ContentEncoding);
            form.Add(decodedKey,
                  HttpUtility.UrlDecode(value.ToString(), request.ContentEncoding));

            key.Length = 0;
            value.Length = 0;
        }
        void LoadWwwForm()
        {
            using (Stream input = GetSubStream(request.InputStream))
            {
                using (StreamReader s = new StreamReader(input, request.ContentEncoding))
                {
                    StringBuilder key = new StringBuilder();
                    StringBuilder value = new StringBuilder();
                    int c;

                    while ((c = s.Read()) != -1)
                    {
                        if (c == '=')
                        {
                            value.Length = 0;
                            while ((c = s.Read()) != -1)
                            {
                                if (c == '&')
                                {
                                    AddRawKeyValue(key, value);
                                    break;
                                }
                                else
                                    value.Append((char)c);
                            }
                            if (c == -1)
                            {
                                AddRawKeyValue(key, value);
                                return;
                            }
                        }
                        else if (c == '&')
                            AddRawKeyValue(key, value);
                        else
                            key.Append((char)c);
                    }
                    if (c == -1)
                        AddRawKeyValue(key, value);

                    EndSubStream(input);
                }
            }
        }

        private void LoadMultiPart()
        {
            string boundary = null;
            if (request.ContentType.Length > 31)
            {
                boundary =  request.ContentType.Substring(30);
            }
            //string boundary = GetParameter(ContentType, "; boundary=");
            if (boundary == null)
                return;

            Stream input = GetSubStream(request.InputStream);
            HttpMultipart multi_part = new HttpMultipart(input, boundary, request.ContentEncoding);

            HttpMultipart.Element e;
            while ((e = multi_part.ReadNextElement()) != null)
            {
                if (e.Filename == null)
                {
                    byte[] copy = new byte[e.Length];

                    input.Position = e.Start;
                    input.Read(copy, 0, (int)e.Length);

                    form.Add(e.Name, request.ContentEncoding.GetString(copy));
                }
                else
                {
                    
                    //
                    // We use a substream, as in 2.x we will support large uploads streamed to disk,
                    //
                    PostedFile sub = new PostedFile(e.Filename, e.ContentType, input, e.Start, e.Length);

                    files.Add(e.Name, sub);
                }
            }
            EndSubStream(input);
        }

        static void EndSubStream(Stream stream)
        {
            
        }

        public object InternalRequest
        {
            get { return request; }
        }

        public IResponse Response
        {
            get { return response; }
        }


        public string Method
        {
            get { return request.HttpMethod; }
        }

        public bool IsLocal
        {
            get { return request.IsLocal; }
        }

        public string UserAgent
        {
            get { return request.UserAgent; }
        }

        public Dictionary<string, System.Net.Cookie> Cookies
        {
            get { return cookies; }
        }

       // public Dictionary<string, object> Items
       // {
      //      get { return items; }
       // }

        public string ResponseType
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string GetContent()
        {
            using (var reader = new StreamReader(request.InputStream))
                {
                    return reader.ReadToEnd();
                }
        }
        public string Data
        {
            get
            {
                if (this.data == null)
                    this.data = GetContent();
                return data;
            }
        }

        public string RawUrl
        {
            get
            {
                return request.RawUrl;
            }
        }
        public Uri Url
        {
            get
            {
                return request.Url;
            }
        }

        public string Host
        {
            get { return request.UserHostAddress; }
        }

        public string RemoteIP
        {
            get { return request.RemoteEndPoint.Address.ToString(); }
        }

        public bool IsSecure
        {
            get { return request.IsSecureConnection; }
        }

        public long Length
        {
            get { return request.ContentLength64; }
        }

        public Uri UrlReferrer
        {
            get { return request.UrlReferrer; }
        }

        public string Authorization
        {
            get { return request.Headers["Authorization"]; }
        }



        public Dictionary<string, string> Form
        {
            get
            {
                if (form == null)
                {
                    files = new Dictionary<string, IPostedFile>();
                    if (request.ContentType != null)
                    {
                        if (request.ContentType.StartsWith("multipart/form-data"))
                        {
                            // form = new WebROCollection();
                            LoadMultiPart();
                            // form.Protect();
                        }
                        else
                        {
                            LoadWwwForm();
                        }
                    }
                    
                }
                return form;
            }
        }

        public Dictionary<string, string> QueryParameters
        {
            get { throw new NotImplementedException(); }
        }
    }
}
