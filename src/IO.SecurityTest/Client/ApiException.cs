using System;

namespace IO.SecurityTest.Client
{
    public class ApiException : Exception
    {
        public int ErrorCode { get; set; }
        public dynamic ErrorContent { get; private set; }
        public ApiException() {}
        public ApiException(int errorCode, string message) : base(message)
        {
            this.ErrorCode = errorCode;
        }
        public ApiException(int errorCode, string message, dynamic errorContent = null) : base(message)
        {
            this.ErrorCode = errorCode;
            this.ErrorContent = errorContent;
        }
    }
}
