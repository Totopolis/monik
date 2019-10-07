using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Monik.Common;

namespace Monik.Service
{
    public class AzureActiveQueue : IActiveQueue
    {
        private QueueClient _client;

        public void Start(EventQueue config, ActiveQueueContext context)
        {
            _client = new QueueClient(config.ConnectionString, config.QueueName);

            _client.RegisterMessageHandler((message, token) =>
            {
                try
                {
                    var buf = message.GetBody<byte[]>();
                    var msg = Event.Parser.ParseFrom(buf);

                    context.OnReceivedMessage(msg);
                }
                catch (Exception ex)
                {
                    context.OnError($"AzureActiveQueue - not able to handle message: {ex}");
                }

                return Task.CompletedTask;
            }, (args =>
            {
                context.OnError($"AzureActiveQueue - exception received: {args.Exception}");
                return Task.CompletedTask;
            }));
        }

        public void Stop()
        {
            _client?.CloseAsync().Wait();
        }
    }
}
