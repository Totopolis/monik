using System;
using System.Configuration;
using System.Linq;
using System.Text;
using Gerakul.FastSql;
using Monik.Client;
using Monik.Common;
using Monik.Service;
using MonikService.Core.Core;
using MonikTestConsoleGenerator.Metrics;

namespace MonikTestConsoleGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var connstr = ConfigurationManager.AppSettings["DBConnectionString"];

            var proto = new { name = default(string), value = default(string) };
            var settings = SimpleCommand.ExecuteQueryAnonymous(proto, connstr, "select Name, Value from mon.Settings").ToDictionary(p=>p.name, p=>p.value);

            var asureSender = new AzureParallelSender(settings["OutcomingConnectionString"], settings["OutcomingQueue"]);

            var monikTestGeneratorInstance = new MonikTestGeneratorInstance(asureSender, new ClientSettings()
            {
                AutoKeepAliveEnable = false,
                SourceName = "TestSource",
                InstanceName = "TestInstance"
            });

            //var logsSender = new InstancesLogsSender(10000, source, monikTestGeneratorInstance);
            //logsSender.StartSendingLogs();

            var metricSender = new MetricsSender(monikTestGeneratorInstance, TimeSpan.FromMinutes(5));
            metricSender.StartSendingMetrics();
        }
    }
}
