using System;

namespace Checker.Types
{
    public class TcpConfig : HostConfig
    {
        public UInt16 Port { get; }

        public TcpConfig(string host, UInt16 port, int rate = 10, int timeout = 10) : base(host, rate, timeout)
        {
            Port = port;
        }
    }
}