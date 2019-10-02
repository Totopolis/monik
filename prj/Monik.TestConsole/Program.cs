using System.Configuration;
using Monik.Client;

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
                    SourceName = ConfigurationManager.AppSettings["SourceName"],
                    InstanceName = ConfigurationManager.AppSettings["InstanceName"]
                });

            client.LogicInfo("Test");

            client.OnStop();
        }
    }
}