using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADIS.Core.ComponentServices;
using ADIS.Core.ComponentServices.Services;
using ADIS.Core.Configuration;
using ADIS.Services;

namespace ConsoleHostExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var cm = ConfigurationManager.Current;
            var host = new HostedAppHost();
            var cs = ComponentServices.Fetch("Services");
            var router = cs.Resolve<IServiceRouter>();
            router.Add("/test/<group1>/<group2>", new EchoResponder());
            router.Add("/test2/<group1>/<group2>/", new EchoResponder());
            router.Add("/<group1>/<group2>", new EchoResponder());
            router.Add("<group1>/<group2>", new EchoResponder());
            host.Initialize();
            host.Start("http://localhost:82/");
            Console.WriteLine("Started listening");

            Console.ReadLine();
        }
    }
}
