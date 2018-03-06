using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gerakul.FastSql;
using Monik.Client;
using Monik.Service;
using MonikService.Core.Core;

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

            var source = new Source()
            {
                Created = DateTime.Now,
                Name = "TestConsole"
            };

            var monikTestGeneratorInstance = new MonikTestGeneratorInstance(asureSender, new ClientSettings()
            {
                AutoKeepAliveEnable = true,
                SourceName = source.Name,
            });

            var instances = new List<InstanceGenerator>();
            for (var i = 0; i < 10000; i++)
            {
                var instName = $"TestInst_{i}";
                instances.Add(new InstanceGenerator()
                {
                    Instance = new Instance()
                    {
                        Created = DateTime.Now,
                        Name = instName,
                        
                    },
                    Source = source,
                    ClientControl = monikTestGeneratorInstance
                });
            }

            while (true)
            {
                for (var counter = 0; counter < instances.Count; counter++)
                {
                    var instanceGenerator = instances[counter];
                    instanceGenerator.ClientControl.LogicInfo(instanceGenerator.Instance.Name + " sends some message at " + DateTime.Now, instanceGenerator.Instance);

                    if (counter % 1000 == 0)
                        Task.Delay(1000).Wait();
                }
            }
        }
    }

    public class InstanceGenerator
    {
        public Instance       Instance      { get; set; }
        public Source         Source        { get; set; }
        public MonikTestGeneratorInstance ClientControl { get; set; }
    }
}
