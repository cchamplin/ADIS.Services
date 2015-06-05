using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADIS.Core.ComponentServices;
using ADIS.Core.ComponentServices.Services;
using log4net;

namespace ADIS.Services
{
    public abstract class ServiceBase : IService
    {
        private IServiceRouterResolver router;
        private ILog log = LogManager.GetLogger(typeof(ServiceBase));


        protected ServiceBase()
        {
            var cs = ADIS.Core.ComponentServices.ComponentServices.Fetch("Services");
             
            this.router = cs.Resolve<IServiceRouter>() as IServiceRouterResolver;
        }

        public virtual IService Initialize()
        {
            var services = ComponentServices.Fetch("Services");
            if (services.ServiceTypeRegistered(typeof(IService)))
            {
                log.Warn("Service is already intializied, and another is being registered");
            }
            services.RegisterOrReplace(typeof(IService), this);

            return this;
        }



        public virtual IService Start(string urlEndpoint)
        {
            throw new NotImplementedException("Start method cannot be called on this type of service");
        }

        public IServiceRouterResolver Router
        {
            get
            {
                return router;
            }
        }

        public virtual IHttpRequest CurrentRequest { get { throw new NotImplementedException("Cannot pull active singleton request"); } }

        public virtual void Dispose()
        {
            var services = ComponentServices.Fetch("Services");
           
            services.Unregister(typeof(IService));
        }
    }
}
