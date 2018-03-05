using System;
using Monik.Client;

namespace Monik.Service
{
    public class ServiceClientSettings : IClientSettings
    {
        private readonly IServiceSettings _serviceSettings;

        public ServiceClientSettings(IServiceSettings aServiceSettings)
        {
            _serviceSettings = aServiceSettings;
        }

        public bool AutoKeepAliveEnable
        {
            get { return true; }
            set { throw new NotImplementedException(); }
        }

        public ushort AutoKeepAliveInterval
        {
            get { return 60; }
            set { throw new NotImplementedException(); }
        }

        public string InstanceName
        {
            get { return _serviceSettings.InstanceName; }
            set { throw new NotImplementedException(); }
        }

        public ushort SendDelay
        {
            get { return 1; }
            set { throw new NotImplementedException(); }
        }

        public string SourceName
        {
            get { return "Monik"; }
            set { throw new NotImplementedException(); }
        }
    }
}