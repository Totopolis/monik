using System.Collections.Generic;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using Google.Protobuf;
using Monik.Common;

namespace Monik.Client.RabbitMQ
{
    public class RabbitMqSender : IMonikSender
    {
        private readonly string _connectionString;
        private readonly string _queueName;

        public RabbitMqSender(string connectionString, string queueName)
        {
            _connectionString = connectionString;
            _queueName = queueName;
        }

        public async Task SendMessages(IEnumerable<Event> events)
        {
            using (var client = RabbitHutch.CreateBus(_connectionString).Advanced)
            {
                foreach (var ev in events)
                {
                    var body = ev.ToByteArray();

                    await client.PublishAsync(Exchange.GetDefault(),
                        _queueName, false,
                        new MessageProperties(),
                        body);
                }
            }
        }
    }
}
