using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Azure.ServiceBus;
using Monik.Common;

namespace Monik.Client
{
    public class AzureSender : IMonikSender
    {
        private const long MaxBatchSize = 262144; // 256 KB
        private const long BatchSizeLimit = MaxBatchSize / 4;

        private readonly Lazy<QueueClient> _client;

        public AzureSender(string connectionString, string queueName)
        {
            _client = new Lazy<QueueClient>(() => new QueueClient(connectionString, queueName));
        }

        public async Task SendMessages(IEnumerable<Event> events)
        {
            var messages = events
                .Select(x => new Message(x.ToByteArray()))
                .ToList();

            await _client.Value.SendBatchAsync(messages, BatchSizeLimit);
        }

        public void Dispose()
        {
            if (_client.IsValueCreated)
            {
                _client.Value.CloseAsync().Wait();
            }
        }
    }
}