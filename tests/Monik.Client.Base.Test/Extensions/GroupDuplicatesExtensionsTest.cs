using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Monik.Common;
using NUnit.Framework;

namespace Monik.Client.Base.Test.Extensions
{
    [TestFixture]
    public class GroupDoublingExtensionsTest
    {
        [Test]
        public void GroupDuplicates_FoldLogs()
        {
            const int minCreated = 1;
            const int n = 3000;
            var log = new Log
            {
                Body = "body",
                Level = LevelType.Application,
                Severity = SeverityType.Error,
            };
            var messages = Enumerable.Range(minCreated, n)
                .Select(i => new Event { Created = i, Lg = log.Clone(), })
                .ToList();

            var result = messages.GroupDuplicates();

            result.Should().BeEquivalentTo(new List<Event>
            {
                new Event
                {
                    Created = minCreated,
                    Lg = new Log
                    {
                        Body = string.Format(GroupDuplicatesExtensions.LogDuplicatesFormat, n, log.Body),
                        Level = log.Level,
                        Severity = log.Severity,
                    },
                }
            });
        }

        [Test]
        public void GroupDuplicates_FoldLogs_Window()
        {
            const int minCreated = 1;
            const int n = 49999;
            var bucketsCount = (int) Math.Ceiling((double) n / GroupDuplicatesExtensions.LogGroupInterval);
            var fullBuckets = bucketsCount > 1
                ? Enumerable.Repeat(GroupDuplicatesExtensions.LogGroupInterval, bucketsCount - 1)
                : Enumerable.Empty<long>();
            var buckets = fullBuckets.Concat(new[] {n % GroupDuplicatesExtensions.LogGroupInterval});
            var log = new Log
            {
                Body = "body",
                Level = LevelType.Application,
                Severity = SeverityType.Error,
            };
            var messages = Enumerable.Range(minCreated, n)
                .Select(i => new Event {Created = i, Lg = log.Clone(),})
                .ToList();

            var result = messages.GroupDuplicates();

            result.Should().BeEquivalentTo(
                buckets.Select((x, idx) => new Event
                {
                    Created = minCreated + idx * GroupDuplicatesExtensions.LogGroupInterval,
                    Lg = new Log
                    {
                        Body = string.Format(GroupDuplicatesExtensions.LogDuplicatesFormat, x, log.Body),
                        Level = log.Level,
                        Severity = log.Severity,
                    },
                })
            );
        }

        [Test]
        public void GroupDuplicates_LogsAndKeepAlive()
        {
            var body = "body";
            var messages = new List<Event>
            {
                new Event
                {
                    Created = 1,
                    Lg = new Log
                    {
                        Body = body,
                        Level = LevelType.Application,
                        Severity = SeverityType.Error,
                    },
                },
                new Event
                {
                    Created = 2,
                    Ka = new KeepAlive {Interval = 1},
                },
                new Event
                {
                    Created = 3,
                    Ka = new KeepAlive {Interval = 1},
                },
                new Event
                {
                    Created = 4,
                    Lg = new Log
                    {
                        Body = body,
                        Level = LevelType.Application,
                        Severity = SeverityType.Error,
                    },
                },
            };

            var result = messages.GroupDuplicates();

            result.Should().BeEquivalentTo(new List<Event>
            {
                new Event
                {
                    Created = 3,
                    Ka = new KeepAlive {Interval = 1},
                },
                new Event
                {
                    Created = 1,
                    Lg = new Log
                    {
                        Body = string.Format(GroupDuplicatesExtensions.LogDuplicatesFormat, 2, body),
                        Level = LevelType.Application,
                        Severity = SeverityType.Error,
                    },
                }
            });
        }
    }
}