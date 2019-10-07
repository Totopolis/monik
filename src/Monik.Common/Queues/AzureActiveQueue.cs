using System;
using Monik.Common;

namespace Monik.Service
{
    public class AzureActiveQueue : IActiveQueue
    {
        private Microsoft.ServiceBus.Messaging.QueueClient _client;

        public void Start(EventQueue config, ActiveQueueContext context)
        {
            _client = Microsoft.ServiceBus.Messaging.QueueClient.
                CreateFromConnectionString(config.ConnectionString, config.QueueName);

            _client.OnMessage(message =>
            {
                try
                {
                    var buf = message.GetBody<byte[]>();
                    var msg = Event.Parser.ParseFrom(buf);

                    context.OnReceivedMessage(msg);
                }
                catch (Exception ex)
                {
                    context.OnError($"MessagePump.OnMessage ServiceBus Parse Error: {ex.Message}");
                }
            });
        }

        public void Stop()
        {
            _client?.Close();
        }
    }
}
