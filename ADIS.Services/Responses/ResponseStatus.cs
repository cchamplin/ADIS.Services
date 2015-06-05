using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIS.Services.Responses
{
    public class ResponseStatus
    {
        protected string errorCode;
        protected string message;
        public ResponseStatus(string errorCode)
        {
            this.errorCode = errorCode;
        }
        public ResponseStatus(string errorCode, string message)
        {
            this.errorCode = errorCode;
            this.message = message;
        }
        public string ErrorCode
        {
            get { return errorCode; }
        }
        public string Message
        {
            get
            {
                return message;
            }
        }
    }
}
