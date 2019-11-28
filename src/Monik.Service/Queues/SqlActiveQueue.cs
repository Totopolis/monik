using System;
using System.Linq;
using System.Threading.Tasks;
using Monik.Common;

namespace Monik.Service
{
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
                    var messages = data.Select(msg => Event.Parser.ParseFrom((byte[]) msg.Body));
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