using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Autofac;
using Monik.Common;

namespace Monik.Service
{
    public class MonikEmbedded : MonikDelayedSender
    {
        private readonly IMonikServiceSettings _settings;
        private readonly ILifetimeScope _autofac;

        public const string SourceName = "Monik";
        public const int AutoKeepAliveInterval = 60; // in sec
        public const ushort SendDelay = 1; // in sec

        public MonikEmbedded(IMonikServiceSettings settings, ILifetimeScope autofac)
            : base(SourceName, settings.InstanceName, AutoKeepAliveInterval, SendDelay)
        {
            _settings = settings;
            _autofac = autofac;
        }

        private IMessagePump _pump = null;

        protected override void OnSend(ConcurrentQueue<Event> events)
        {
            if (_pump == null)
                _pump = _autofac.Resolve<IMessagePump>();

            _pump.OnEmbeddedEvents(events);
        }

    }//end of class

    // TODO: move to test project and test bootstrap
    public class MonikEmulator
    {

    }
}
