using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ADIS.Core.ComponentServices;

namespace ADIS.Services
{
    public class HttpHandlerFactory : IHttpHandlerFactory
    {
        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            var container = ComponentServices.Fetch("Services");
            if (!container.ServiceTypeRegistered(typeof(IService)))
            {
                throw new Exception("A service host has not been initialized or registered");
            }

            var service = container.Resolve<IService>();

            var ctx = context.Request.RequestContext.HttpContext;

            var request = new AspRequest(ctx);

            var pathInfo = ctx.Request.PathInfo;
            System.Diagnostics.Debug.WriteLine(requestType + " | " + url + " | " + pathTranslated);

            var route = service.Router.ResolveRoute(request);
            if (route != null)
            {
                System.Diagnostics.Debug.WriteLine("Creating handler from route");
                return route.CreateHandler();
            }
           

  

            return null;
        }
        public static IHttpHandler GetHandler(IHttpRequest request)
        {
            var container = ComponentServices.Fetch("Services");
            if (!container.ServiceTypeRegistered(typeof(IService)))
            {
                throw new Exception("A service host has not been initialized or registered");
            }

            var service = container.Resolve<IService>();

           
            //var pathInfo = ctx.Request.PathInfo;
            //System.Diagnostics.Debug.WriteLine(requestType + " | " + url + " | " + pathTranslated);

            var route = service.Router.ResolveRoute(request);
            if (route != null)
            {
                System.Diagnostics.Debug.WriteLine("Creating handler from route");
                return route.CreateHandler();
            }




            throw new Exception("Unable to resolve route");
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
            
        }
    }
}
