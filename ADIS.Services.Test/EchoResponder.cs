using ADIS.Core.ComponentServices.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIS.Services.Test
{
    public class EchoResponder : IRequestHandler
    {
        public object Handle(IRequest request, IRoute route)
        {
            var response = new Response();
            response.RequestType = request.Method;
            response.RequestParameters = new List<string>();
            response.RequestParameters.Add("Echo");
            foreach (var file in request.Files) {
                var sr = new StreamReader(file.Value.InputStream);
                response.Files.Add(file.Value.FileName + ": " + sr.ReadToEnd());
            }
            return response;
        }
    }
}
