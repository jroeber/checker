using System;

namespace Checker.Types
{
    public struct Report
    {
        public readonly int Id;
        public readonly DateTime TimeStamp;
        public readonly bool Okay;
        public readonly string Type;
        public readonly string Host;
        public readonly UInt16? Port;
        public readonly string Message;

        public Report(bool okay, string type, string host, UInt16? port = null, string message = "")
        {
            Id = (type + host + port.ToString()).GetHashCode();
            TimeStamp = DateTime.Now;
            Okay = okay;
            Type = type;
            Host = host;
            Port = port;
            Message = message;
        }

        public string SocketAddress
            => Host + ((Port != null) ? ":" + Port.ToString() : "");
    }
}