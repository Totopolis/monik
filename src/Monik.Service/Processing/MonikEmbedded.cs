using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Monik.Common;

namespace Monik.Service
{
    public class MonikEmbedded : MonikDelayedSender
    {
        private readonly IMonikServiceSettings _settings;
        private readonly ILifetimeScope _autofac;

        private const string SourceName = "Monik";
        private const int AutoKeepAliveInterval = 60; // in sec
        private const ushort SendDelay = 1; // in sec
        private const int WaitTimeOnStop = 10_000;
        private const bool GroupDuplicates = true;
        private const int QueueCapacity = 100_000;

        public MonikEmbedded(IMonikServiceSettings settings, ILifetimeScope autofac)
            : base(SourceName, settings.InstanceName,
                AutoKeepAliveInterval, SendDelay, WaitTimeOnStop,
                GroupDuplicates, QueueCapacity)
        {
            _settings = settings;
            _autofac = autofac;
        }

        private IMessagePump _pump = null;

        protected override Task OnSend(IEnumerable<Event> events)
        {
            if (_pump == null)
                _pump = _autofac.Resolve<IMessagePump>();

            _pump.OnEmbeddedEvents(events);
            return Task.CompletedTask;
        }

    }//end of class
}
