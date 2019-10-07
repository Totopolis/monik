using System.Collections.Concurrent;
using System.IO;
using Google.Protobuf;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.InteropExtensions;
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
                    var message = new Message(Serialize(arr));
                    client.SendAsync(message).Wait();
                }
            }
            finally
            {
                client.CloseAsync().Wait();
            }
        }

        private static byte[] Serialize<T>(T obj)
        {
            var serializer = DataContractBinarySerializer<T>.Instance;
            using (var memoryStream = new MemoryStream(256))
            {
                serializer.WriteObject(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }
    }
}
