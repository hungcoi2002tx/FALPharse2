using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.ExceptionModels
{
    public class BadRequestException : Exception
    {
        public int ErrorCode { get; set; }
        public BadRequestException(int errorCode)
        {
            ErrorCode = errorCode;
        }

        public BadRequestException(string message) : base(message) { }
        public BadRequestException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
