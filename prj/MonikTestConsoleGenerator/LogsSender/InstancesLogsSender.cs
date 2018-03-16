using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonikService.Core.Repository;

namespace MonikTestConsoleGenerator.LogsSender
{
    public class InstancesLogsSender
    {
        private List<InstanceGenerator> InstanceGenerators { get; set; }
        private CancellationTokenSource cancellationTokenSource { get; set; } = new CancellationTokenSource();

        public InstancesLogsSender(int instancesCount, MonikTestGeneratorInstance monikTestGeneratorInstance)
        {
            for (var i = 0; i < instancesCount; i++)
            {
                var instName = $"TestInst_{i}";
                InstanceGenerators.Add(new InstanceGenerator()
                {
                    Instance = new Instance()
                    {
                        Created = DateTime.Now,
                        Name = instName,

                    },
                    ClientControl = monikTestGeneratorInstance
                });
            }
        }

        public void StartSendingLogs()
        {
            var cancellationToken = cancellationTokenSource.Token;
            Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    for (var counter = 0; !cancellationToken.IsCancellationRequested && counter < InstanceGenerators.Count; counter++)
                    {
                        var instanceGenerator = InstanceGenerators[counter];
                        instanceGenerator.ClientControl.LogicInfo(
                            instanceGenerator.Instance.Name + " sends some message at " + DateTime.Now,
                            instanceGenerator.Instance.Name);

                        if (counter % 100 == 0)
                            Task.Delay(1000, cancellationToken).Wait(cancellationToken);
                    }
                }
            }, cancellationToken);
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }
    }
}