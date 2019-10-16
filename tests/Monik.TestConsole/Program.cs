using System.Configuration;
using Monik.Client;
using Monik.Common;

namespace Monik.TestConsole
{
    class Program
    {
        static void Main()
        {
            var client = new MonikClient(
                new AzureSender(
                    ConfigurationManager.AppSettings["ConnectionString"],
                    ConfigurationManager.AppSettings["QueueName"]
                ),
                new ClientSettings
                {
                    AutoKeepAliveEnable = true,
                    SourceName = ConfigurationManager.AppSettings["SourceName"],
                    InstanceName = ConfigurationManager.AppSettings["InstanceName"]
                });

            client.LogicInfo("Test");
            client.Measure("Metric_Gauge", AggregationType.Gauge, 100);
            client.Measure("Metric_Accum", AggregationType.Accumulator, 100);

            client.OnStop();
        }
    }
}