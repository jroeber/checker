using System;

namespace Checker.Types
{
    public class HttpConfig : TcpConfig
    {
        public int ExpectedCode { get; }

        public HttpConfig(string host, UInt16 port = 80, int expectedCode = 200, int rate = 10, int timeout = 10) : base(host, port, rate, timeout)
        {
            ExpectedCode = expectedCode;
        }
    }
}