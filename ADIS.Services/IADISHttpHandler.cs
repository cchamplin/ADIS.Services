using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIS.Services
{
    public interface IADISHttpHandler
    {
        Task AsyncProcessRequest(IHttpRequest request, IResponse response);
    }
}
