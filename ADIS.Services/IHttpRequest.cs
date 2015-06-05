using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADIS.Core.ComponentServices.Services;

namespace ADIS.Services
{
    public interface IHttpRequest : IRequest
    {
        object InternalRequest { get; }
        IResponse Response { get; }

        string Method { get; }

        bool IsLocal { get; }

        string UserAgent { get; }

        Dictionary<string, System.Net.Cookie> Cookies { get; }

        string ResponseType { get; set; }

        

        string GetContent();

        Uri Url { get; }
        string RawUrl { get; }
        string Host { get; }
        string RemoteIP { get; }
        bool IsSecure { get; }

        long Length { get; }

        Uri UrlReferrer { get; }


    }
}
