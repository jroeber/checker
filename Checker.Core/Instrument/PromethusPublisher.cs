using System;
using Checker.Types;
using Prometheus;

namespace Checker.Instrument
{
    class PrometheusPublisher
    {
        static IDisposable? _reportSubscription;
        static MetricServer? _metricServer;
        static Gauge? _hostGauge { get; set; }

        public static void StartMetrics(UInt16 port = 8080)
        {
            _hostGauge = Metrics.CreateGauge("Hosts", "", new GaugeConfiguration{
                LabelNames = new[] { "type", "socketaddress" }
            });

            _metricServer = new MetricServer(port);
            _metricServer.Start();
        }

        public static void SetReportSource(IObservable<Report> reports)
        {
            if (_reportSubscription != null)
            {
                _reportSubscription.Dispose();
            }

            if (_hostGauge != null)
            {
                _reportSubscription = reports.Subscribe(r => {
                    _hostGauge
                        .WithLabels(r.Type, r.SocketAddress)
                        .Set(r.Okay ? 1.0 : 0.0);
                });
            }

            // TODO there's a (very) minor danger of a label getting "stuck" when changing sources, reset them all to 0 here to prevent this
            // looks like I will need to keep track of all labels to do so?
        }
    }
}