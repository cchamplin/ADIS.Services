using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIS.Services.Responses
{
    public class FieldError
    {
        protected string errorCode;
        protected string fieldName;
        protected string message;
        public string ErrorCode { get { return errorCode; } set { errorCode = value; } }
        public string FieldName { get { return fieldName; } set { fieldName = value; } }
        public string Message { get { return message; } set { message = value; } }
    }
}
