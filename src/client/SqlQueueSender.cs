
using Gerakul.SqlQueue.InMemory;
using Google.Protobuf;
using Monik.Common;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Monik.Client
{
    public class SqlQueueSender : IMonikSender
    {
        private readonly string _connectionString;
        private readonly string _queueName;

        public SqlQueueSender(string connectionString, string queueName)
        {
            _connectionString = connectionString;
            _queueName = queueName;
        }

        public void SendMessages(ConcurrentQueue<Event> aQueue)
        {
            var client = QueueClient.Create(_connectionString, _queueName);
            using (var writer = client.CreateWriter())
            {
                var data = new List<byte[]>();
                while (aQueue.TryDequeue(out var msg))
                    data.Add(msg.ToByteArray());

                writer.WriteMany(data);
            }
        }
    }
}
