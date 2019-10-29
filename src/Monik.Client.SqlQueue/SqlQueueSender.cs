using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gerakul.SqlQueue.InMemory;
using Google.Protobuf;
using Monik.Common;

namespace Monik.Client
{
    public class SqlQueueSender : IMonikSender
    {
        private readonly Lazy<Writer> _client;

        public SqlQueueSender(string connectionString, string queueName)
        {
            _client = new Lazy<Writer>(() =>
                QueueClient
                    .Create(connectionString, queueName)
                    .CreateWriter());
        }

        public Task SendMessages(IEnumerable<Event> events)
        {
            return Task.Run(() =>
            {
                var data = events.Select(x => x.ToByteArray());
                _client.Value.WriteMany(data);
            });
        }

        public void Dispose()
        {
            if (_client.IsValueCreated)
            {
                _client.Value.Close();
            }
        }
    }
}