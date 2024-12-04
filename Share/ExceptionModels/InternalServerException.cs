using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.ExceptionModels
{
    public class InternalServerException : Exception
    {
        public int ErrorCode { get; set; }
        public InternalServerException(int errorCode)
        {
            ErrorCode = errorCode;
        }

        public InternalServerException(string message) : base(message) { }
        public InternalServerException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
