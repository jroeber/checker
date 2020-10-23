using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Checker.Types;

namespace Checker.Checkers
{
    public class CheckerBuilder
    {
        public static IConnectableObservable<Report> Build(IEnumerable<HostConfig> conf)
            => conf
                .Select(hostConfig => Build(hostConfig))
                .Aggregate( (x, y) => x.Merge(y))
                .Publish();

        public static IObservable<Report> Build(HostConfig conf)
        {
            switch (conf)
            {
                case PingConfig p: return Pinger.Create(p);
                case TcpConfig t:
                    switch (t)
                    {
                        case HttpConfig h: return HttpConnector.Create(h);
                        default: return TcpConnector.Create(t);
                    }
                default:
                    throw new ArgumentException($"Unsupported HostConfig type: {conf.GetType()}");
            }
        }
    }
}