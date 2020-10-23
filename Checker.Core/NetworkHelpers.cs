using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using Checker.Types;

namespace Checker
{
    public static class NetworkHelpers
    {
        public static NetInfo PrimaryNetInfo
            => new NetInfo{ 
                IP = PrimaryIP,
                Netmask = PrimaryNetmask,
                Gateway = PrimaryGateway,
                DNS = PrimaryDNS
            };

        public static IPAddress PrimaryIP
            => PrimaryIPInformation != null ? PrimaryIPInformation.Address : NoAddress;

        public static string PrimaryIPcidr
            => PrimaryIP.ToString() + "/" + NetmaskBits(PrimaryNetmask);

        public static IPAddress PrimaryGateway
            => PrimaryGatewayInformation != null ? PrimaryGatewayInformation.Address : NoAddress;

        public static IPAddress PrimaryNetmask
            => PrimaryIPInformation != null ? PrimaryIPInformation.IPv4Mask : NoAddress;

        public static IPAddress[] PrimaryDNS
            => PrimaryInterface
                .GetIPProperties()
                .DnsAddresses
                .ToArray();

        public static IObservable<EventPattern<EventArgs>> AddressChanged
            => Observable.FromEventPattern<NetworkAddressChangedEventHandler, EventArgs>(
                handler => (s, a) => handler(s, a),
                handler => NetworkChange.NetworkAddressChanged += handler,
                handler => NetworkChange.NetworkAddressChanged -= handler
               );

        public static IObservable<NetInfo> PrimaryNetUpdates
            => AddressChanged.Select(_ => new NetInfo{ 
                IP = PrimaryIP,
                Netmask = PrimaryNetmask,
                Gateway = PrimaryGateway,
                DNS = PrimaryDNS
            });

        // Liberated from https://stackoverflow.com/a/8711667
        // If you try to put a non-netmask into here, you're gonna have a bad time
        public static int NetmaskBits(IPAddress netmask)
        {
            int totalBits = 0;
            foreach (string octet in netmask.ToString().Split('.'))
            {
                byte octetByte = byte.Parse(octet);
                while (octetByte != 0)
                {
                    totalBits += octetByte & 1;
                    octetByte >>= 1;
                }                
            }
            return totalBits;
        }

        static bool HasGateway(this NetworkInterface adapter)
            => adapter.GetIPProperties().GatewayAddresses.Count > 0;

        static NetworkInterface PrimaryInterface
            => NetworkInterface.GetAllNetworkInterfaces().AsEnumerable()
                .Where(adapter => adapter.HasGateway())
                .FirstOrDefault() ?? NetworkInterface.GetAllNetworkInterfaces().AsEnumerable().First();

        static UnicastIPAddressInformation PrimaryIPInformation
            => PrimaryInterface
                .GetIPProperties()
                .UnicastAddresses
                .FirstOrDefault();

        static GatewayIPAddressInformation PrimaryGatewayInformation
            =>  PrimaryInterface
                .GetIPProperties()
                .GatewayAddresses
                .FirstOrDefault();

        public static IPAddress NoAddress
            => new IPAddress(new byte[]{0, 0, 0, 0});

        public static (string Host, UInt16 Port) SplitSocketAddress(string socketAddress)
        {
            var colonLocation = socketAddress.LastIndexOf(':');
            if (colonLocation == -1)
            {
                // no colon; it's just a host
                return (socketAddress, 0);
            }
            else
            {
                var host = socketAddress.Substring(0, colonLocation);
                var port = Convert.ToUInt16(socketAddress.Substring(colonLocation + 1));
                return (host, port);
            }
        }
    }
}