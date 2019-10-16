using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Monik.Client.Base.Test")]

namespace Monik.Common
{
    internal static class GroupDuplicatesExtensions
    {
        internal const string LogDuplicatesFormat = "|x{0}| {1}";
        internal const long LogGroupInterval = 5_000; // ms

        private static readonly Event.MsgOneofCase[] PrioritiesArray =
        {
            Event.MsgOneofCase.Ka,
            Event.MsgOneofCase.Mc,
            Event.MsgOneofCase.Pc,
            Event.MsgOneofCase.Lg,
            Event.MsgOneofCase.None,
        };

        private static readonly Dictionary<Event.MsgOneofCase, int> Priorities =
            PrioritiesArray.Select((x, idx) => new {x, idx}).ToDictionary(v => v.x, v => v.idx);

        public static IList<Event> GroupDuplicates(this IList<Event> messages,
            string logDuplicatesFormat = LogDuplicatesFormat)
            => messages
                .GroupBy(x => x.MsgCase)
                .OrderBy(x => Priorities[x.Key])
                .SelectMany(x =>
                {
                    switch (x.Key)
                    {
                        case Event.MsgOneofCase.Ka:
                            var latestKeepAlive = x.Aggregate((i, j) => i.Created > j.Created ? i : j);
                            return new[] {latestKeepAlive};
                        case Event.MsgOneofCase.Lg:
                            return x
                                .GroupBy(v => new {v.Lg.Body, v.Lg.Severity, v.Lg.Level})
                                .SelectMany(g =>
                                {
                                    var min = g.Min(r => r.Created);
                                    return g
                                        .GroupBy(r => (r.Created - min) / LogGroupInterval)
                                        .Select(rg =>
                                        {
                                            var first = rg.First();

                                            var count = rg.Count();
                                            if (count > 1)
                                            {
                                                first.Lg.Body = string.Format(
                                                    logDuplicatesFormat,
                                                    count, first.Lg.Body);
                                            }

                                            return first;
                                        });
                                });
                        default:
                            return x;
                    }
                })
                .ToList();
    }
}