using System.Collections.Concurrent;
using System.Collections.Generic;
using Google.Protobuf;
using Microsoft.ServiceBus.Messaging;
using Monik.Client;
using Monik.Common;

namespace MonikTestConsoleGenerator
{
    public class AzureParallelSender : IClientSender
    {
        private readonly string _serviceBusConnectionString;
        private readonly string _queueName;
        private readonly QueueClient _client;

        public AzureParallelSender(string aServiceBusConnectionString, string aQueueName)
        {
            _serviceBusConnectionString = aServiceBusConnectionString;
            _queueName = aQueueName;

            _client = QueueClient.CreateFromConnectionString(_serviceBusConnectionString, _queueName);
        }

        public void SendMessages(ConcurrentQueue<Event> aQueue)
        {
            var messages = new List<BrokeredMessage>();

            while (aQueue.TryDequeue(out var msg))
            {
                var arr = msg.ToByteArray();
                messages.Add(new BrokeredMessage(arr));
            }

            _client.SendBatchAsync(messages);
        }
    }
}