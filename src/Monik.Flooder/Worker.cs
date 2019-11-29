using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monik.Client;
using Monik.Common;
using Monik.Flooder.Settings;

namespace Monik.Flooder
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMonik _monik;
        private readonly int _delay;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            var settings = configuration.GetSection("Monik").Get<MonikSettings>();
            var sender = CreateSender(settings.SenderType, settings.SenderSettings);
            _monik = new MonikClient(sender, settings.ClientSettings);
            _delay = settings.Delay;
        }

        private static IMonikSender CreateSender(MonikSenderType type, MonikSenderSettings settings)
        {
            return type switch
            {
                MonikSenderType.Azure => (IMonikSender) new AzureSender(settings.ConnectionString, settings.QueueName),
                MonikSenderType.RabbitMq => new RabbitMqSender(settings.ConnectionString, settings.QueueName),
                _ => throw new ArgumentException($"Unsupported client type {type}")
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var counter = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                _monik.LogicVerbose("verbose");
                _monik.LogicInfo("info");
                _monik.LogicWarning("warning");
                _monik.LogicError("error");

                _monik.Measure("acc", AggregationType.Accumulator, 1);
                _monik.Measure("gau", AggregationType.Gauge, counter);

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                counter = (counter + 1) % 100;

                try
                {
                    await Task.Delay(_delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // ignore
                }
            }
            _monik.OnStop();
        }
    }
}