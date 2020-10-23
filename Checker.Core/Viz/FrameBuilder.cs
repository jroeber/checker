using System.Collections.Generic;
using System.Reactive.Linq;
using System;
using System.Text;
using System.Linq;
using Checker.Types;

namespace Checker.Viz
{
    public static class FrameBuilder
    {
        public static string GetFrame(int width, int height, IEnumerable<Report> reports, NetInfo netInfo)
        {
            StringBuilder sb = new StringBuilder();

            var niceNetInfo = NetInfoPrintable.Create(netInfo);
            sb.Append(MakeHeader(width, niceNetInfo.IPandNetmask, niceNetInfo.Gateway, niceNetInfo.DNS));

            var optimalWidths = getOptimalWidths(width, reports);

            var sorted = reports
                .OrderBy(report => report.Okay)
                .ThenBy(report => report.Host)
                .ThenBy(report => report.Type)
                .Take(height - 8);

            foreach (var report in sorted)
            {
                sb.AppendLine(
                    makeFormattedReport(
                        report,
                        optimalWidths.status,
                        optimalWidths.proto,
                        optimalWidths.host,
                        optimalWidths.message)
                    .SetWidth(width));
            }

            if (reports.Count() > height - 8) sb.Append("[More rows...]");

            return sb.ToString();
        }

        static string MakeHeader(int maxWidth, string ip, string gateway, string dns, string dtFormat = "dd MMM yyy HH:mm:ss")
        {
            var dt = DateTime.Now.ToString(dtFormat);

            var sb = new StringBuilder();
            sb.AppendLine(dt.SetWidth(maxWidth));
            sb.AppendLine(("IP:      " + ip).SetWidth(maxWidth));
            sb.AppendLine(("Gateway: " + gateway).SetWidth(maxWidth));
            sb.AppendLine(("DNS:     " + dns).SetWidth(maxWidth));
            sb.AppendLine(new string('-', maxWidth));
            return sb.ToString();
        }

        static (int status, int proto, int host, int message) getOptimalWidths(int maxWidth, IEnumerable<Report> reports, string separator = " | ")
        {
            // First generate some information
            int maxStatusLength = 11;
            int maxProtoLength = reports
                .Select(r => r.Type.Length)
                .DefaultIfEmpty()
                .Max();
            int maxHostLength = reports
                .Select(r => makeHost(r.Host, r.Port).Length)
                .DefaultIfEmpty()
                .Max();
            int maxMessageLength = reports
                .Select(r => r.Message.Length)
                .DefaultIfEmpty()
                .Max();

            int numberOfSeparatorsNeeded = (maxMessageLength > 0 ? 3 : 2);
            int idealTotalColumnSpace = maxStatusLength + maxProtoLength + maxHostLength + maxMessageLength + (numberOfSeparatorsNeeded * separator.Length);

            // Then decide what to do...
            if (idealTotalColumnSpace <= maxWidth)
            {
                return (maxStatusLength, maxProtoLength, maxHostLength, maxMessageLength);
            }
            else
            {
                // Reserve space for status, type, and the separators
                int remainingSpace = maxWidth - maxStatusLength - 3 - maxProtoLength - 3 - (maxMessageLength > 0 ? 3 : 0);

                int neededSpace = maxHostLength + maxMessageLength;
                double overageRatio = (double) remainingSpace / neededSpace;
                int newHostLength = (int) Math.Round(maxHostLength * overageRatio);
                int newMessageLength = (int) Math.Round(maxMessageLength * overageRatio);

                return (maxStatusLength, maxProtoLength, newHostLength, newMessageLength);
            }
        }

        static string makeFormattedReport(Report report, int statusWidth, int typeWidth, int hostWidth, int messageWidth, string separator = " | ")
        {
            StringBuilder sb = new StringBuilder();
            bool needSeparator = false;

            if (statusWidth > 0)
            {
                sb.Append(makeStatus(report.Okay, report.TimeStamp, statusWidth));
                needSeparator = true;
            }

            if (typeWidth > 0)
            {
                sb.Append(needSeparator ? separator : "");
                sb.Append(makeType(report.Type, typeWidth));
                needSeparator = true;
            }

            if (hostWidth > 0)
            {
                sb.Append(needSeparator ? separator : "");
                sb.Append(makeHost(report.Host, report.Port, hostWidth));
                needSeparator = true;
            }

            if (messageWidth > 0)
            {
                sb.Append(needSeparator ? separator : "");
                sb.Append(makeMessage(report.Message, messageWidth));
            }

            return sb.ToString();
        }

        static string makeStatus(bool okay, DateTime timeStamp, int width)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(okay ? "✔️" : "✖️");
            sb.Append("  ");
            sb.Append(timeStamp.SimpleElapsed());
            sb.Append(" ago");
            return sb.ToString().SetWidth(width);
        }

        static string makeType(string type, int width)
            => type.SetWidth(width);

        static string makeHost(string host, UInt16? port, int? width = null)
        {
            string rtn = (port == null) ? host : host + ":" + port.ToString();
            return (width != null ? rtn.SetWidth(width.Value) : rtn);
        }

        static string makeMessage(string message, int width)
            => message.SetWidth(width);
    }
}