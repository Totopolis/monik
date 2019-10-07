using System;
using System.Collections.Generic;
using System.Configuration;

namespace Monik.Service
{
    public class MonikServiceSettings : IMonikServiceSettings
    {
        // TODO: use concurent when online updates
        private static Dictionary<string, string> _settings = null;

        public void UpdateSettings(Dictionary<string, string> val)
        {
            _settings = val;
        }

        public string InstanceName
        {
            get { return ConfigurationManager.AppSettings["InstanceName"]; }
        }

        public string DbConnectionString
        {
            get { return ConfigurationManager.AppSettings["DBConnectionString"]; }
        }

        public int CleanupBatchSize => int.Parse(ConfigurationManager.AppSettings["CleanupBatchSize"]);
        public int WriteBatchSize => int.Parse(ConfigurationManager.AppSettings["WriteBatchSize"]);
        public int WriteBatchTimeout => int.Parse(ConfigurationManager.AppSettings["WriteBatchTimeout"]);

        public int DayDeepKeepAlive
        {
            get { return int.Parse(_settings["DayDeepKeepAlive"]); }
        }

        public int DayDeepLog
        {
            get { return int.Parse(_settings["DayDeepLog"]); }
        }

        public string OutcomingConnectionString
        {
            get { return _settings["OutcomingConnectionString"]; }
        }

        public string OutcomingQueue
        {
            get { return _settings["OutcomingQueue"]; }
        }


        public string AuthSecretKey => ConfigurationManager.AppSettings["AuthSecretKey"];
    } //end of class
}
