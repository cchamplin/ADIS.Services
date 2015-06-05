using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADIS.Core.ComponentServices.Services;

namespace ADIS.Services
{
    public interface IServiceRouterResolver : IServiceRouter
    {
        IRequestRoute ResolveRoute(IHttpRequest request);
    }
}
