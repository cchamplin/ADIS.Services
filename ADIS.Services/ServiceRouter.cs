using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADIS.Core.ComponentServices.Services;

namespace ADIS.Services
{
    public class ServiceRouter : IServiceRouterResolver
    {
        protected List<IRequestRoute> routes;
        public ServiceRouter()
        {
            routes = new List<IRequestRoute>();
        }
        public IRequestRoute ResolveRoute(IHttpRequest request)
        {
            foreach (var route in routes)
            {
                if (route.Handles(request))
                    return route;
            }
            return null;
        }

        public IRoute Add(string route, IRequestHandler handler)
        {
            var routeItem = new Route(route,handler,RequestMethod.DELETE | RequestMethod.POST | RequestMethod.GET | RequestMethod.PUT);
            routes.Add(routeItem);
            return routeItem;
        }

        public IRoute Add(string route, IRequestHandler handler, RequestMethod method)
        {
            var routeItem = new Route(route, handler, method);
            routes.Add(routeItem);
            return routeItem;
        }
    }
}
