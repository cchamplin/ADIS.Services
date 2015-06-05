using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ADIS.Services
{
    /// <summary>
    /// For services hosted inside IIS
    /// </summary>
    public abstract class AppHostBase : ServiceBase
    {
        public AppHostBase()
        {
        }
        public override IHttpRequest CurrentRequest
        {
            get
            {
                return HttpContext.Current.AsRequest();
            }
        }
    }
}
