using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ADIS.Services
{
    public static class HttpRequestExtensions
    {
        public static IHttpRequest AsRequest(this HttpContext context)
        {
            if (context == null)
                throw new NotImplementedException("Not available in self hosted applications");

            return new AspRequest(context.Request.RequestContext.HttpContext);
        }
        public static IHttpRequest AsRequest(this HttpListenerContext context)
        {
            if (context == null)
                throw new NotImplementedException("Not available in iss hosted applications");

            return new ListenerRequest(context);
        }
    }
}
