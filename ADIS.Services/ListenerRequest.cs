using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ADIS.Services
{
    public class ListenerRequest : IHttpRequest
    {
        protected readonly HttpListenerRequest request;
        protected readonly IResponse response; 
        
        protected Dictionary<string, Cookie> cookies;
        protected Dictionary<string, object> items;
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

        public Dictionary<string, object> Items
        {
            get { return items; }
        }

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

        public string RawUrl
        {
            get
            {
                return request.RawUrl;
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
    }
}
