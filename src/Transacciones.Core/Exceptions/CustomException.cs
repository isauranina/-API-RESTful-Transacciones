using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class CustomException : ApplicationException
    {
        //public int ErrorCode { get; }
        public HttpStatusCode StatusCode { get; }

        public CustomException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)//CustomException(string message, int errorCode) : base(message)
            : base(message)
        {
            //this.ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }
}
