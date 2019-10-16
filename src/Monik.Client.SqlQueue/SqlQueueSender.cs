
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
        private readonly string _connectionString;
        private readonly string _queueName;

        public SqlQueueSender(string connectionString, string queueName)
        {
            _connectionString = connectionString;
            _queueName = queueName;
        }

        public Task SendMessages(IEnumerable<Event> events)
        {
            return Task.Run(() =>
            {
                var client = QueueClient.Create(_connectionString, _queueName);
                using (var writer = client.CreateWriter())
                {
                    var data = events.Select(x => x.ToByteArray());
                    writer.WriteMany(data);
                }
            });
        }
    }
}
