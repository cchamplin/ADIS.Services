using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ADIS.Services
{
    public interface IResponse : ADIS.Core.ComponentServices.Services.IResponse
    {
        object InternalRequest { get; }
        int StatusCode { get; set; }
        string ContentType { get; set; }
        void AddHeader(string name, string value);
        void Redirect(string url);

        Stream OutputStream { get; }

        void Write(string text);

        void Close();

        void Flush();

        bool Closed { get; }

        void SetLength(long length);

        bool KeepAlive { get; set; }
    }
}
