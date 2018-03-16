using System.Collections.Concurrent;
using Google.Protobuf;
using Microsoft.ServiceBus.Messaging;
using Monik.Common;

namespace Monik.Client
{
    public class AzureSender : IClientSender
    {
        private readonly string _serviceBusConnectionString;
        private readonly string _queueName;

        public AzureSender(string aServiceBusConnectionString, string aQueueName)
        {
            _serviceBusConnectionString = aServiceBusConnectionString;
            _queueName                  = aQueueName;
        }

        public void SendMessages(ConcurrentQueue<Event> aQueue)
        {
            var client = QueueClient.CreateFromConnectionString(_serviceBusConnectionString, _queueName);

            try
            {
                Event msg;

                while (aQueue.TryDequeue(out msg))
                {
                    var arr     = msg.ToByteArray();
                    var message = new BrokeredMessage(arr);
                    client.Send(message);
                }
            }
            finally
            {
                client.Close();
            }
        }
    }
}