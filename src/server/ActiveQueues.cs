using EasyNetQ;
using Monik.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

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

    public class SqlActiveQueue : IActiveQueue
    {
        private const string SqlQueueSubscription = "Monik";

        private Gerakul.SqlQueue.InMemory.QueueClient _client;
        private Gerakul.SqlQueue.InMemory.AutoReader _reader;

        public void Start(EventQueue config, ActiveQueueContext context)
        {
            _client = Gerakul.SqlQueue.InMemory.QueueClient
                .Create(config.ConnectionString, config.QueueName);

            var subscriptionId = _client.FindSubscription(SqlQueueSubscription);
            if (subscriptionId == 0)
                _client.CreateSubscription(SqlQueueSubscription);

            _reader = _client.CreateAutoReader(SqlQueueSubscription);

            _reader.Start((data) => Task.Factory.StartNew(() =>
            {
                try
                {
                    var messages = data.Select(msg => Event.Parser.ParseFrom(msg.Body));
                    context.OnReceivedMessages(messages);
                }
                catch (Exception ex)
                {
                    context.OnError($"MessagePump.OnMessage SqlQueue Parse Error: {ex.Message}");
                }
            })).Wait();
        }

        public void Stop()
        {
            _reader?.Stop().Wait();
            _reader?.Close();

            _client?.DeleteSubscription(SqlQueueSubscription); // TODO: is it needed?
        }
    }
}
