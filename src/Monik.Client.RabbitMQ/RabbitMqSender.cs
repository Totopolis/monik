using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using Google.Protobuf;
using Monik.Common;

namespace Monik.Client
{
    public class RabbitMqSender : IMonikSender
    {
        private readonly string _queueName;
        private readonly Lazy<IAdvancedBus> _client;

        public RabbitMqSender(string connectionString, string queueName)
        {
            _queueName = queueName;
            _client = new Lazy<IAdvancedBus>(() => RabbitHutch.CreateBus(connectionString).Advanced);
        }

        public async Task SendMessages(IEnumerable<Event> events)
        {
            if (!_client.Value.IsConnected)
                return;

            foreach (var ev in events)
            {
                var body = ev.ToByteArray();

                await _client.Value.PublishAsync(Exchange.GetDefault(),
                    _queueName, false,
                    new MessageProperties(),
                    body);
            }
        }

        public void Dispose()
        {
            if (_client.IsValueCreated)
            {
                _client.Value.Dispose();
            }
        }
    }
}