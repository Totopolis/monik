using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Monik.Common;

namespace Monik.Service
{
    public class AzureActiveQueue : IActiveQueue
    {
        private const int MaxMessageCount = 200;
        private const int PrefetchCount = 400;
        private const int TimeoutOnException = 1_000; // ms
        private const int ReceiverTimeoutOnExit = 10_000; // ms

        private IMessageReceiver _receiver;
        private Task _receiverTask;
        private CancellationTokenSource _receiverTokenSource;
        private readonly Dictionary<string, DateTime> _fallbacks = new Dictionary<string, DateTime>();

        public void Start(QueueReaderSettings config, ActiveQueueContext context)
        {
            _receiver = new MessageReceiver(
                new ServiceBusConnection(config.ConnectionString),
                config.QueueName,
                prefetchCount: PrefetchCount
            );
            
            _receiverTokenSource = new CancellationTokenSource();
            var completeTime = DateTime.UtcNow;
            _receiverTask = Task.Run(async () => {
                while (!_receiverTokenSource.IsCancellationRequested)
                {
                    var receiveTime = DateTime.UtcNow;
                    context.OnMeasure("AzureCTime", (receiveTime - completeTime).TotalMilliseconds);
                    IList<Message> messages;
                    try
                    {
                        messages = await _receiver.ReceiveAsync(MaxMessageCount);
                        if (messages == null || messages.Count == 0)
                            continue;
                    }
                    catch (Exception ex)
                    {
                        context.OnError($"AzureActiveQueue - exception received: {ex}");
                        try
                        {
                            await Task.Delay(TimeoutOnException, _receiverTokenSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            // ignore
                        }
                        continue;
                    }

                    var processTime = DateTime.UtcNow;
                    context.OnMeasure("AzureRTime", (processTime - receiveTime).TotalMilliseconds);
                    context.OnMeasure("AzureBSize", messages.Count);

                    var lockTokens = messages.Select(x => x.SystemProperties.LockToken);
                    var completeMessagesTask = _receiver.CompleteAsync(lockTokens);

                    foreach (var message in messages)
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
                    }

                    completeTime = DateTime.UtcNow;
                    context.OnMeasure("AzurePTime", (completeTime - processTime).TotalMilliseconds);

                    try
                    {
                        await completeMessagesTask;
                    }
                    catch (Exception ex)
                    {
                        context.OnError($"Failed to complete messages: {ex}");
                    }
                }
            });
        }

        public void Stop()
        {
            _receiverTokenSource?.Cancel();
            _receiverTask?.Wait(ReceiverTimeoutOnExit);
            _receiver?.CloseAsync().Wait();
        }
    }
}
