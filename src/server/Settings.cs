using Gerakul.FastSql;
using System;
using System.Collections.Generic;
using Microsoft.Azure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Monik.Client;

namespace Monik.Service
{
    public class ServiceSettings : IServiceSettings
    {
        // TODO: use concurent when online updates
        private static Dictionary<string, string> _settings = null;

        public void OnStart()
        {
            if (_settings == null)
            {
                _settings = new Dictionary<string, string>();

                var proto = new { name = default(string), value = default(string) };
                var settings = SimpleCommand.ExecuteQueryAnonymous(proto, DbConnectionString, "select Name, Value from mon.Settings");
                foreach (var it in settings)
                    _settings[it.name] = it.value;
            }
        }

        public void OnStop()
        {
        }

        public string CloudInstanceName
        {
            get { return RoleEnvironment.IsEmulated ? "Dev" : "Azure"; }
        }

        public int DayDeepKeepAlive
        {
            get { return int.Parse(_settings["DayDeepKeepAlive"]); }
        }

        public int DayDeepLog
        {
            get { return int.Parse(_settings["DayDeepLog"]); }
        }

        public string DbConnectionString
        {
            get { return CloudConfigurationManager.GetSetting("DBConnectionString"); }
        }

        public string OutcomingConnectionString
        {
            get { return _settings["OutcomingConnectionString"]; }
        }

        public string OutcomingQueue
        {
            get { return _settings["OutcomingQueue"]; }
        }

    } //end of class

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
            get { return _serviceSettings.CloudInstanceName; }
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
