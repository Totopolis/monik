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

        private readonly string _serviceBusConnectionString;
        private readonly string _queueName;

        public AzureSender(string aServiceBusConnectionString, string aQueueName)
        {
            _serviceBusConnectionString = aServiceBusConnectionString;
            _queueName = aQueueName;
        }

        public async Task SendMessages(IEnumerable<Event> events)
        {
            var client = new QueueClient(_serviceBusConnectionString, _queueName);

            try
            {
                var messages = events
                    .Select(x => new Message(x.ToByteArray()))
                    .ToList();

                await client.SendBatchAsync(messages, BatchSizeLimit);
            }
            finally
            {
                await client.CloseAsync();
            }
        }
    }
}
