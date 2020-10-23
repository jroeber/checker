using System;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Checker.Types;

namespace Checker.Checkers
{
    class HttpConnector
    {
        public const string TYPE = "HTTP";

        public static IObservable<Report> Create(HttpConfig config)
        {
            string host = config.Host;
            UInt16 port = config.Port;
            int expectedCode = config.ExpectedCode;
            int rate = config.Rate * 1000;
            int timeout = config.Timeout * 1000;

            return Create(host, port, expectedCode, rate, timeout);
        }

        public static IObservable<Report> Create(
            string host, 
            UInt16 port = 80, 
            int expectedCode = 200, 
            int rateMs = 10000, 
            int timeoutMs = 10000)
        {
            return Observable.Create<Report>(async o =>
            {
                var url = "http://" + host + ':' + port;

                while (true)
                {
                    using (var client = new HttpClient())
                    try
                    {
                        client.Timeout = TimeSpan.FromMilliseconds(timeoutMs);
                        HttpResponseMessage response = await client.GetAsync(url);
                        if ((int) response.StatusCode == expectedCode)
                        {
                            o.OnNext(new Report(
                                okay: true,
                                type: TYPE,
                                host: host,
                                port: port,
                                message: $"{(int) response.StatusCode} {response.StatusCode}"
                            ));
                        }
                        else
                        {
                            o.OnNext(new Report(
                                okay: false,
                                type: TYPE,
                                host: host,
                                port: port,
                                message: $"Expected {expectedCode}, got {(int) response.StatusCode}"
                            ));
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        // Timed out
                        o.OnNext(new Report(
                            okay: false,
                            type: TYPE,
                            host: host,
                            port: port,
                            message: $"Timed out after {timeoutMs} ms"
                        ));
                    }
                    catch (Exception e)
                    {
                        o.OnNext(new Report(
                            okay: false,
                            type: TYPE,
                            host: host,
                            port: port,
                            message: $"{e.Message}"
                        ));
                    }
                    await Task.Delay(rateMs); 
                } // while (true)
            }); // Observable
        }
    }
}