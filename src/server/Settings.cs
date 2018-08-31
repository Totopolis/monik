﻿using Gerakul.FastSql;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monik.Service
{
    public class MonikServiceSettings : IMonikServiceSettings
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

        public string InstanceName
        {
            get { return ConfigurationManager.AppSettings["InstanceName"]; }
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

        public string DbConnectionString
        {
            get { return ConfigurationManager.AppSettings["DBConnectionString"]; }
        }

        public string OutcomingConnectionString
        {
            get { return _settings["OutcomingConnectionString"]; }
        }

        public string OutcomingQueue
        {
            get { return _settings["OutcomingQueue"]; }
        }


        public string AuthSecretKeyBase64 => ConfigurationManager.AppSettings["AuthSecretKeyBase64"];
        public byte[] AuthSecretKey => Convert.FromBase64String(AuthSecretKeyBase64);

    } //end of class
}
