using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ADIS.Services
{
    public interface IService
    {
        IService Initialize();
        IService Start(string urlEndpoint);
        IServiceRouterResolver Router { get; }
    }
}
