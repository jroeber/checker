using System;
using System.Collections.Generic;
using System.Net;

namespace Checker.Types
{
    public static class Config
    {
        public static IEnumerable<HostConfig> Create(RawConfig rawConfig) // Will also include DnsConfigs eventually
        {
            var _hostConfigs = new List<HostConfig>();

            foreach (var rawHostConf in rawConfig.Hosts)
            {
                var (host, port) = NetworkHelpers.SplitSocketAddress(rawHostConf.Host);

                switch (rawHostConf.Type.ToLower(), port)
                {
                    case ("tcp", _): // tcp, any port
                        _hostConfigs.Add(new TcpConfig(host, port, rawHostConf.Rate, rawHostConf.Timeout));
                        break;
                    case ("", 80): // no type, port 80
                    case ("http", _): // http, any port
                        if (port == 0) port = 80;
                        _hostConfigs.Add(new HttpConfig(host, port, rawHostConf.ExpectedCode, rawHostConf.Rate, rawHostConf.Timeout));
                        break;
                    case ("", _): // no type, any port
                        if (port != 0)
                            _hostConfigs.Add(new TcpConfig(host, port, rawHostConf.Rate, rawHostConf.Timeout));
                        else
                            _hostConfigs.Add(new PingConfig(host, rawHostConf.Rate, rawHostConf.Timeout));
                        break;
                    case ("ping", _): // ping, any port
                        _hostConfigs.Add(new PingConfig(host, rawHostConf.Rate, rawHostConf.Timeout));
                        break;
                    case (_, _):
                        break;
                }
            }

            // TODO foreach (var rawDnsConf in rawConfig.Dns)...

            return _hostConfigs;
        }
    }
}