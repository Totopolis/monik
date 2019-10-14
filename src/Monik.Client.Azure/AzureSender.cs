using System.Collections.Concurrent;
using Google.Protobuf;
using Microsoft.Azure.ServiceBus;
using Monik.Common;

namespace Monik.Client
{
    public class AzureSender : IMonikSender
    {
        private readonly string _serviceBusConnectionString;
        private readonly string _queueName;

        public AzureSender(string aServiceBusConnectionString, string aQueueName)
        {
            _serviceBusConnectionString = aServiceBusConnectionString;
            _queueName = aQueueName;
        }

        public void SendMessages(ConcurrentQueue<Event> aQueue)
        {
            var client = new QueueClient(_serviceBusConnectionString, _queueName);

            try
            {
                while (aQueue.TryDequeue(out var msg))
                {
                    var arr = msg.ToByteArray();
                    var message = new Message(arr);
                    client.SendAsync(message).Wait();
                }
            }
            finally
            {
                client.CloseAsync().Wait();
            }
        }
    }
}
