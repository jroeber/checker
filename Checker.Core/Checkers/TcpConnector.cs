using System;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Checker.Types;

namespace Checker.Checkers
{
    public static class TcpConnector
    {
        public const string TYPE = "TCP";

        public static IObservable<Report> Create(TcpConfig config)
        {
            string host = config.Host;
            UInt16 port = config.Port;
            int rate = config.Rate * 1000;
            int timeout = config.Timeout * 1000;

            return Create(host, port, rate, timeout);
        }

        public static IObservable<Report> Create(string host, UInt16 port = 80, int rateMs = 10000, int timeoutMs = 10000)
        {
            return Observable.Create<Report>(async o =>
            {
                while (true)
                {
                    using (var client = new TcpClient())
                    try
                    {
                        var finishedOnTime = client.ConnectAsync(host, port).Wait(timeoutMs);
                        if (finishedOnTime && client.Connected)
                        {
                            o.OnNext(new Report(okay: true,
                                type: TYPE,
                                host: host,
                                port: port));
                        }
                        else
                        {
                            o.OnNext(new Report(okay: false,
                                type: TYPE,
                                host: host,
                                port: port,
                                message: $"Timed out after {timeoutMs} ms"));
                            client.Close();
                        }
                    }
                    catch (AggregateException e)
                    {
                        if (e.InnerException != null)
                        {
                            o.OnNext(new Report(okay: false,
                                type: TYPE,
                                host: host,
                                port: port,
                                message: e.InnerException.Message));
                        }
                        else
                        {
                            // Multiple exceptions?
                            o.OnNext(new Report(okay: false,
                                type: TYPE,
                                host: host,
                                port: port,
                                message: e.Message));
                        }
                    }
                    catch (Exception e) // Handles SocketException (e.g. network unreachable)
                    {
                        o.OnNext(new Report(okay: false,
                            type: TYPE,
                            host: host,
                            port: port,
                            message: e.Message
                        ));
                    }
                    await Task.Delay(rateMs);
                } // while (true)
            });
        }
    }
}