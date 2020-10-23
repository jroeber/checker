using System.Net;

namespace Checker.Types
{
    public class NetInfo
    {
        public IPAddress IP { get; set; } = NetworkHelpers.NoAddress;
        public IPAddress Netmask { get; set; } = NetworkHelpers.NoAddress;
        public IPAddress Gateway { get; set; } = NetworkHelpers.NoAddress;
        public IPAddress[] DNS { get; set; } = new IPAddress[]{ NetworkHelpers.NoAddress };
    }
}