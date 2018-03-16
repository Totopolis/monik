using System;
using Monik.Client.Settings;

namespace MonikService.Core.Settings
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
            get => true;
            set => throw new NotImplementedException();
        }

        public ushort AutoKeepAliveInterval
        {
            get => 60;
            set => throw new NotImplementedException();
        }

        public string InstanceName
        {
            get => _serviceSettings.InstanceName;
            set => throw new NotImplementedException();
        }

        public ushort SendDelay
        {
            get => 1;
            set => throw new NotImplementedException();
        }

        public string SourceName
        {
            get => "Monik";
            set => throw new NotImplementedException();
        }
    }
}