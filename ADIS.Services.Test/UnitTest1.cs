using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ADIS.Core.Configuration;
using ADIS.Core.ComponentServices;
using ADIS.Core.ComponentServices.Services;
using System.Net;
using FastSerialize;
using System.Text;

namespace ADIS.Services.Test
{
    [TestClass]
    public class UnitTest1
    {
        private void SetupServer()
        {
            var cm = ConfigurationManager.Current;
            var host = new HostedAppHost();
            var cs = ComponentServices.Fetch("Services");
            var router = cs.Resolve<IServiceRouter>();
            router.Add("/test", new EchoResponder());
            host.Initialize();
            host.Start("http://localhost:82/");
            Console.WriteLine("Started listening");
        }
        [TestMethod]
        public void TestGet()
        {
            //SetupServer();
            var sr = new Serializer(typeof(FastSerialize.JsonSerializerGeneric));
            var request = WebRequest.Create("http://localhost:82/test");
            request.Method = "GET";
            var response = (HttpWebResponse)request.GetResponse();
            var responseData = sr.Deserialize<Response>(response.GetResponseStream());
            System.Diagnostics.Debug.WriteLine("Got Response");
        }

           [TestMethod]
        public void TestGetWithQuery()
        {
            //SetupServer();
            var sr = new Serializer(typeof(FastSerialize.JsonSerializerGeneric));
            var getData = "item1=test&item2=testmore";
            var request = WebRequest.Create("http://localhost:82/test?"+getData);
            request.Method = "GET";
            var response = (HttpWebResponse)request.GetResponse();
            var responseData = sr.Deserialize<Response>(response.GetResponseStream());
            System.Diagnostics.Debug.WriteLine("Got Response");
        }

           [TestMethod]
        public void TestPost()
        {
            //SetupServer();
            var sr = new Serializer(typeof(FastSerialize.JsonSerializerGeneric));
            var request = WebRequest.Create("http://localhost:82/test");
            request.Method = "POST";
            var postData = "item1=test&item2=testmore";
            var encoded = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = encoded.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(encoded, 0, encoded.Length);
            }
            var response = (HttpWebResponse)request.GetResponse();
            var responseData = sr.Deserialize<Response>(response.GetResponseStream());
            System.Diagnostics.Debug.WriteLine("Got Response");
        }
    }
}
