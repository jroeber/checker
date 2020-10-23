using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;
using Checker.Types;
using Checker.Checkers;
using Checker.Viz;
using System.Collections.Generic;
using Checker.Instrument;
using System.Threading;

namespace Checker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand();

            Option config = new Option("--config", "Path to the config file")
            {
                Argument = new Argument<string>("path")
            };
            config.AddAlias("-c");
            config.Required = true;

            Option prometheusPort = new Option("--prometheus-port", "Prometheus endpoint port, default 8080")
            {
                Argument = new Argument<UInt16>("port", 8080)
            };
            prometheusPort.AddAlias("-p");
            prometheusPort.Required = false;

            Option noPrometheus = new Option<bool>("--no-prometheus", "Don't run a Prometheus endpoint"){};
            noPrometheus.Required = false;

            Option noConsole = new Option<bool>("--no-console", "Suppress console visualization"){};
            noConsole.Required = false;

            rootCommand.AddOption(config);
            rootCommand.AddOption(prometheusPort);
            rootCommand.AddOption(noPrometheus);
            rootCommand.AddOption(noConsole);

            rootCommand.Handler = CommandHandler.Create<string, UInt16, bool, bool>(Run);
            await rootCommand.InvokeAsync(args);

            Environment.Exit(0);
        }

        static void Run(string config, UInt16 prometheusPort = 8080, bool noPrometheus = false, bool noConsole = false)
        {
            var rawConfig = RawConfig.Create(config);
            var configObj = Config.Create(rawConfig);
            var netInfo = NetworkHelpers.PrimaryNetUpdates;
            var usePrometheus = !noPrometheus;

            if (usePrometheus)
            {
                PrometheusPublisher.StartMetrics(prometheusPort);
                System.Console.WriteLine($"Prometheus endpoint listening on port {prometheusPort}");
            }

            var vizSubscription = InitMetricsAndViz(configObj, netInfo, usePrometheus, noConsole);

            NetworkHelpers.AddressChanged.Subscribe(_ => {
                if (vizSubscription != null) vizSubscription.Dispose();
                vizSubscription = InitMetricsAndViz(configObj, netInfo, usePrometheus, noConsole);
            });

            if (noPrometheus && noConsole)
            {
                System.Console.WriteLine("Warning: Checks are being performed, but both console visualization and prometheus endpoint are disabled!");
                System.Console.WriteLine("         This is a pointless waste of CPU and bandwidth.");
            }
            Thread.Sleep(Timeout.Infinite);
        }

        static IDisposable? InitMetricsAndViz(
            IEnumerable<HostConfig> configObj,
            IObservable<NetInfo> netInfo,
            bool usePrometheus = true,
            bool noConsole = false)
        {
            var reports = CheckerBuilder.Build(configObj);

            if (usePrometheus)
            {
                PrometheusPublisher.SetReportSource(reports);
            }

            IDisposable? vizSubscription = null;
            if (!noConsole)
            {
                var viz = new VizEngine(reports, netInfo);
                vizSubscription = viz.Frames
                    .Subscribe(frame => {
                        Console.Clear();
                        System.Console.WriteLine(frame);
                    });
            }

            reports.Connect();
            return vizSubscription;
        }
    }
}
