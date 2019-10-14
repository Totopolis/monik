using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Monik.Common;

namespace Monik.Service
{
    public class AzureActiveQueue : IActiveQueue
    {
        private QueueClient _client;
        private readonly Dictionary<string, DateTime> _fallbacks = new Dictionary<string, DateTime>();

        public void Start(EventQueue config, ActiveQueueContext context)
        {
            _client = new QueueClient(config.ConnectionString, config.QueueName);

            _client.RegisterMessageHandler((message, token) =>
            {
                Event msg = null;
                try
                {
                    msg = Event.Parser.ParseFrom(message.Body);
                }
                catch (Exception)
                {
                    // ToDo: remove temporal fallback
                    try
                    {
                        var buf = message.GetBody<byte[]>();
                        msg = Event.Parser.ParseFrom(buf);
                    }
                    catch (Exception ex)
                    {
                        context.OnError($"AzureActiveQueue - not able to handle message: {ex}");
                    }

                    if (msg != null)
                    {
                        var curDate = DateTime.UtcNow;
                        var name = $"{msg.Source}::{msg.Instance}";
                        if (!_fallbacks.TryGetValue(name, out var date) || (curDate - date).TotalMinutes > 30)
                        {
                            context.OnVerbose($"[AzureActiveQueue] {name} used obsolete xml serialized Event");
                            _fallbacks[name] = curDate;
                        }
                    }
                }

                if (msg != null)
                {
                    context.OnReceivedMessage(msg);
                }

                return Task.CompletedTask;
            }, (args =>
            {
                context.OnError($"AzureActiveQueue - exception received: {args.Exception}");
                return Task.CompletedTask;
            }));
        }

        public void Stop()
        {
            _client?.CloseAsync().Wait();
        }
    }
}
