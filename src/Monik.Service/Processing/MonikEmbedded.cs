using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Monik.Common;

namespace Monik.Service
{
    public class MonikEmbedded : MonikDelayedSender
    {
        private readonly ILifetimeScope _autofac;
        private readonly ILogger _logger;

        private const string SourceName = "Monik";
        private const int AutoKeepAliveInterval = 60; // in sec
        private const ushort SendDelay = 1; // in sec
        private const int WaitTimeOnStop = 10_000;
        private const bool GroupDuplicates = true;
        private const int QueueCapacity = 100_000;

        public MonikEmbedded(IMonikServiceSettings settings, ILifetimeScope autofac, ILogger<MonikEmbedded> logger)
            : base(SourceName, settings.InstanceName,
                AutoKeepAliveInterval, SendDelay, WaitTimeOnStop,
                GroupDuplicates, QueueCapacity)
        {
            _autofac = autofac;
            _logger = logger;
        }

        private IMessagePump _pump;

        protected override Task OnSend(IEnumerable<Event> events)
        {
            _pump ??= _autofac.Resolve<IMessagePump>();

            var eventsList = events.ToList();

            foreach (var log in eventsList
                .Select(x => x.Lg)
                .Where(x => x != null))
            {
                switch (log.Severity)
                {
                    case SeverityType.Fatal:
                        _logger.LogCritical(log.Body);
                        break;
                    case SeverityType.Error:
                        _logger.LogError(log.Body);
                        break;
                    case SeverityType.Warning:
                        _logger.LogWarning(log.Body);
                        break;
                    case SeverityType.Info:
                        _logger.LogInformation(log.Body);
                        break;
                    case SeverityType.Verbose:
                        _logger.LogDebug(log.Body);
                        break;
                    default:
                        _logger.LogTrace(log.Body);
                        break;
                }
            }

            _pump.OnEmbeddedEvents(eventsList);

            return Task.CompletedTask;
        }

    }//end of class
}
