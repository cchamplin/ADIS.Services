using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using ADIS.Core.ComponentServices;
using ADIS.Core.ComponentServices.Services;
using ADIS.Services.Handlers;
using FastSerialize;
namespace ADIS.Services
{
    public class Route : IRequestRoute
    {
        protected string rawPattern;
        protected string regexPattern;
        protected RequestMethod method;
        protected IRequestHandler handler;
        protected Regex matcher;
        protected ISerializer serializer;
        public Route(string pattern, IRequestHandler handler, RequestMethod method)
        {
            this.rawPattern = pattern;
            this.method = method;
            this.regexPattern = "^" + Regex.Escape(this.rawPattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
            this.matcher = new Regex(regexPattern, RegexOptions.Compiled);
            this.handler = handler;

            var services = ComponentServices.Fetch("Text");
            serializer = services.Resolve<ISerializer>();

        }
        public bool HandlesPUT
        {
            get { return (method & RequestMethod.PUT) == RequestMethod.PUT; }
        }

        public bool HandlesGET
        {
            get { return (method & RequestMethod.GET) == RequestMethod.GET; }
        }

        public bool HandlesDELETE
        {
            get { return (method & RequestMethod.DELETE) == RequestMethod.DELETE; }
        }

        public bool HandlesPOST
        {
            get { return (method & RequestMethod.POST) == RequestMethod.POST; }
        }

        string IRoute.Route
        {
            get { return rawPattern; }
        }


        public bool Handles(IHttpRequest request)
        {
            if (matcher.IsMatch(request.RawUrl))
            {
                switch (request.Method.ToUpper())
                {
                    case "POST":
                        return HandlesPOST;
                    case "GET":
                        return HandlesGET;
                    case "PUT":
                        return HandlesPUT;
                    case "DELETE":
                        return HandlesDELETE;
                }
                
            }
            return false;
        }




        public IHttpHandler CreateHandler()
        {
            return new RestfulHandler(handler, this, serializer);
        }
    }
}
