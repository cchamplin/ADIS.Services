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
        protected object handler;
        protected Regex matcher;
        protected ISerializer serializer;
        private Route(string pattern, object handler, RequestMethod method)
        {
            this.rawPattern = pattern;
            this.method = method;

            if (Regex.IsMatch(this.rawPattern, "\\<([a-zA-Z0-9_-]+)\\>\\<([a-zA-Z0-9_-]+)\\>"))
            {
                throw new Exception("Invalid route: route contains adjacent directory capture groups");
            }
            if (Regex.IsMatch(this.rawPattern, "\\[([a-zA-Z0-9_-]+)\\]\\[([a-zA-Z0-9_-]+)\\]"))
            {
                throw new Exception("Invalid route: route contains adjacent directory capture groups");
            }

            var matchPattern = Regex.Replace(Regex.Escape(this.rawPattern).Replace(@"\*", ".*").Replace(@"\?", "."), "\\/?\\<(?<partname>[a-zA-Z0-9_-]+)\\>\\/?", "\\/?(?<${partname}>[^\\/][a-zA-Z0-9\\._-]*)");

            matchPattern = Regex.Replace(matchPattern, "\\/?\\\\\\[(?<partname>[a-zA-Z0-9_-]+)\\]", "\\/?(?<${partname}>[^\\/][a-zA-Z0-9\\._-]+)");
            matchPattern += "\\/?";
            this.regexPattern = "^" + matchPattern + "$";
            System.Diagnostics.Debug.WriteLine(this.rawPattern + " " + matchPattern + " " + regexPattern);
            this.matcher = new Regex(regexPattern, RegexOptions.Compiled);
            this.handler = handler;

            var services = ComponentServices.Fetch("Text");
            serializer = services.Resolve<ISerializer>();
        }
        public Route(string pattern, IRawRequestHandler handler, RequestMethod method) : this(pattern,(object)handler,method)
        {
        }
        public Route(string pattern, IRequestHandler handler, RequestMethod method) : this(pattern,(object)handler,method)
        {
            

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

        Dictionary<string, string> IRoute.GetComponents(Uri uri)
        {
            var match =matcher.Match(uri.AbsolutePath);
            if (match.Success)
            {
                var results = new Dictionary<string, string>();
                foreach (var name in matcher.GetGroupNames())
                {
                    results.Add(name,match.Groups[name].Value);
                }
                return results;
            }
            return null;
        }


        public bool Handles(IHttpRequest request)
        {
            if (matcher.IsMatch(request.Url.AbsolutePath))
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
