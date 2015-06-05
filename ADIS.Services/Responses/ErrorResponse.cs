using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIS.Services.Responses
{
    public class ErrorResponse
    {
        public ErrorResponse(ResponseStatus status) {
            this.status = status;
        }

        public ErrorResponse(ResponseStatus status, string stackTrace)
        {
            this.status = status;
            this.stackTrace = stackTrace;
        }

        protected ResponseStatus status;
        protected string stackTrace;
        public ResponseStatus Status
        {
            get { return status; }
        }

        public List<FieldError> Errors = new List<FieldError>();

        public string StackTrace
        {
            get
            {
                return stackTrace;
            }
        }
    }
}
