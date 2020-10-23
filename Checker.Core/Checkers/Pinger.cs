using System;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Checker.Types;

namespace Checker.Checkers
{
    public static class Pinger
    {
        public const string TYPE = "PING";

        public static IObservable<Report> Create(PingConfig config)
        {
            string host = config.Host;
            int rate = config.Rate * 1000;
            int timeout = config.Timeout * 1000;

            return Pinger.Create(host, rate, timeout);
        }

        public static IObservable<Report> Create(string hostOrAddress, int rateMs = 10000, int timeoutMs = 10000)
        {
            return Observable.Create<Report>(async o =>
            {
                while (true)
                {
                    using (var ping = new Ping())
                    try
                    {
                        PingReply reply = await ping.SendPingAsync(hostOrAddress, timeoutMs);
                        switch (reply.Status)
                        {
                            case IPStatus.Success:
                                o.OnNext(new Report(okay: true, type: TYPE, host: hostOrAddress, message: $"RTT: {reply.RoundtripTime} ms"));
                                break;
                            case IPStatus.TimedOut:
                                o.OnNext(new Report(okay: false, type: TYPE, host: hostOrAddress, message: $"Request timed out"));
                                break;
                            default:
                                o.OnNext(new Report(okay: false, type: TYPE, host: hostOrAddress, message: reply.Status.ToString())); // TODO test if Status.ToString() is useful
                                break;
                        }
                    }
                    catch(PingException e)
                    {
                        var msg = e.InnerException != null ? e.InnerException.Message : "Ping failed (no additional info)";

                        o.OnNext(new Report(
                            okay: false,
                            type: TYPE,
                            host: hostOrAddress,
                            message: msg));
                    }
                    catch(Exception e)
                    {
                        o.OnNext(new Report(
                            okay: false,
                            type: TYPE,
                            host: hostOrAddress,
                            message: e.Message
                        ));
                    }
                    await Task.Delay(rateMs);
                } // while (true)
            });
        }
    }
}