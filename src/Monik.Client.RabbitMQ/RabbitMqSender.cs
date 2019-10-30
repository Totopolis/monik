using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.ConnectionString;
using EasyNetQ.DI;
using EasyNetQ.Topology;
using Google.Protobuf;
using Monik.Common;
using Monik.Common.RabbitMQ;

namespace Monik.Client
{
    public class RabbitMqSender : IMonikSender
    {
        private ConnectionConfiguration _config;
        private readonly Func<IServiceResolver, ConnectionConfiguration> _configFactory;
        private readonly string _queueName;

        public RabbitMqSender(string connectionString, string queueName)
        {
            _queueName = queueName;
            connectionString = connectionString.FetchConnectionSslOptions(out var configure);
            _configFactory = x =>
            {
                if (_config == null)
                {
                    var connectionConfig = x.Resolve<IConnectionStringParser>().Parse(connectionString);
                    _config = configure(connectionConfig);
                }

                return _config;
            };
        }

        public async Task SendMessages(IEnumerable<Event> events)
        {
            using (var client = RabbitHutch.CreateBus(_configFactory, x => { }).Advanced)
            {
                foreach (var ev in events)
                {
                    var body = ev.ToByteArray();

                    await client.PublishAsync(Exchange.GetDefault(),
                        _queueName, false,
                        new MessageProperties(),
                        body);
                }
            }
        }
    }
}