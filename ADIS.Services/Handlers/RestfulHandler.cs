using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADIS.Core.ComponentServices.Services;
using FastSerialize;

namespace ADIS.Services.Handlers
{
    public class RestfulHandler : HandlerBase
    {
        protected object handler;
        protected IRequestRoute route;
        protected ISerializer serializer;
        public RestfulHandler(IRequestHandler handler, IRequestRoute route, ISerializer serializer)
        {
            System.Diagnostics.Debug.WriteLine("Initialize Restful Handler");
            this.handler = handler;
            this.route = route;
            this.serializer = serializer;
        }
        public RestfulHandler(IRawRequestHandler handler, IRequestRoute route, ISerializer serializer)
        {
            System.Diagnostics.Debug.WriteLine("Initialize Restful Handler");
            this.handler = handler;
            this.route = route;
            this.serializer = serializer;
        }
        internal RestfulHandler(object handler, IRequestRoute route, ISerializer serializer)
        {
            System.Diagnostics.Debug.WriteLine("Initialize Restful Handler");
            this.handler = handler;
            this.route = route;
            this.serializer = serializer;
        }
       // public override object CreateRequest(IRequest request)
       // {
       //     throw new NotImplementedException();
       // }
        public override void ProcessRequest(IHttpRequest request, IResponse response)
        {
            if (handler != null)
            {
                if (handler is IRequestHandler)
                {
                    var result = ((IRequestHandler)handler).Handle(request, route);
                    response.Write(serializer.Serialize(result, true, false));
                }
                else if (handler is IRawRequestHandler)
                {
                    ((IRawRequestHandler)handler).Handle(request, route, response);
                }
            }
            else
            {
                throw new Exception("Unable to resolve data handler");
            }
        }

       // public override object GetResponse(IRequest request)
      //  {
       //     throw new NotImplementedException();
       // }
    }
}
