using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ADIS.Core.ComponentServices.Services;

namespace ADIS.Services
{
    public interface IRequestRoute : IRoute
    {
        bool HandlesPUT { get; }
        bool HandlesGET { get; }
        bool HandlesDELETE { get; }
        bool HandlesPOST { get; }

        bool Handles(IHttpRequest request);
        IHttpHandler CreateHandler();


    }
}
