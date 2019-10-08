using System;
using System.Threading.Tasks;
using EasyNetQ;
using Monik.Common;

namespace Monik.Service
{
    public class RabbitActiveQueue : IActiveQueue
    {
        private IAdvancedBus _client;

        public void Start(EventQueue config, ActiveQueueContext context)
        {
            _client = RabbitHutch.CreateBus(config.ConnectionString).Advanced;
            var queue = _client.QueueDeclare(config.QueueName);

            _client.Consume(queue, (body, properties, info) => Task.Factory.StartNew(() =>
            {
                try
                {
                    var msg = Event.Parser.ParseFrom(body);

                    context.OnReceivedMessage(msg);
                }
                catch (Exception ex)
                {
                    context.OnError($"MessagePump.OnMessage RabbitMQ Parse Error: {ex.Message}");
                }
            }));
        }

        public void Stop()
        {
            _client?.Dispose();
        }
    }
}