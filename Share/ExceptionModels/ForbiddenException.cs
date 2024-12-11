using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.ExceptionModels
{
    public class ForbiddenException : Exception
    {
        public int ErrorCode { get; set; }
        public ForbiddenException(int errorCode)
        {
            ErrorCode = errorCode;
        }

        public ForbiddenException(string message) : base(message) { }
        public ForbiddenException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
