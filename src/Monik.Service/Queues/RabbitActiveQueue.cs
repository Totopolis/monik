using System;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.ConnectionString;
using Monik.Common;
using Monik.Common.RabbitMQ;

namespace Monik.Service
{
    public class RabbitActiveQueue : IActiveQueue
    {
        private IAdvancedBus _client;

        public void Start(QueueReaderSettings config, ActiveQueueContext context)
        {
            var connectionString = config.ConnectionString.FetchConnectionSslOptions(out var configure);

            _client = RabbitHutch
                .CreateBus(x =>
                {
                    var connectionConfig = x.Resolve<IConnectionStringParser>().Parse(connectionString);
                    return configure(connectionConfig);
                }, x => { })
                .Advanced;

            var queue = _client.QueueDeclare(config.QueueName);

            _client.Consume(queue, (body, properties, info) => Task.Factory.StartNew(() =>
            {
                try
                {
                    var msg = Event.Parser.ParseFrom(body);

                    context.OnReceivedMessage(msg);
                }
                catch (Exception ex)
                {
                    context.OnError($"MessagePump.OnMessage RabbitMQ Parse Error: {ex.Message}");
                }
            }));
        }

        public void Stop()
        {
            _client?.Dispose();
        }
    }
}