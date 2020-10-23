using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using Checker.Types;

namespace Checker.Viz
{
    public class VizEngine
    {
        ConcurrentDictionary<int, Report> _latestReports;
        NetInfo _netInfo;

        public IObservable<string> Frames;

        public VizEngine(IObservable<Report> combinedReports, IObservable<NetInfo> netInfo, int refreshIntervalMs = 100)
        {
            _latestReports = new ConcurrentDictionary<int, Report>();
            combinedReports.Subscribe(report => {
                _latestReports[report.Id] = report;
            });

            _netInfo = NetworkHelpers.PrimaryNetInfo;
            netInfo.Subscribe(netInfo => _netInfo = netInfo);

            Frames = Observable
                .Interval(TimeSpan.FromMilliseconds(refreshIntervalMs))
                .Select(_ => FrameBuilder.GetFrame(Console.WindowWidth, Console.WindowHeight, _latestReports.Values, _netInfo));
        }
    }
}