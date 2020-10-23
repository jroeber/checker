using System.Text;

namespace Checker.Types
{
    public class NetInfoPrintable
    {
        public string IPandNetmask { get; private set; } = "0.0.0.0/0";
        public string Gateway { get; private set; } = "0.0.0.0";
        public string DNS { get; private set; } = "0.0.0.0";

        public static NetInfoPrintable Create(NetInfo netInfo)
        {
            var ipAndNetmask = netInfo.IP.ToString() + '/' + NetworkHelpers.NetmaskBits(netInfo.Netmask);

            var sb = new StringBuilder();
            foreach (var ip in netInfo.DNS)
            {
                sb.Append(ip.ToString());
                sb.Append(' ');
            }

            return new NetInfoPrintable{
                IPandNetmask = ipAndNetmask,
                Gateway = netInfo.Gateway.ToString(),
                DNS = sb.ToString()
            };
        }
    }
}