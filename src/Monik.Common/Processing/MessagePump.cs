using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Monik.Common;

namespace Monik.Service
{
    public class MessagePump : IMessagePump
    {
        private const int DelayOnException = 500; //in ms
        private const int DelayOnProcess = 500; //in ms
        private const int WaitOnExit = 10_000; //in ms

        private readonly IRepository _repository;
        private readonly ICacheSourceInstance _cache;
        private readonly IMessageProcessor _processor;
        private readonly IMonik _monik;

        private readonly List<IActiveQueue> _queues = new List<IActiveQueue>();

        private readonly ConcurrentQueue<Event> _msgBuffer = new ConcurrentQueue<Event>();

        private Task _pumpTask;
        private readonly ManualResetEvent _newMessageEvent = new ManualResetEvent(false);
        private readonly CancellationTokenSource _pumpCancellationTokenSource = new CancellationTokenSource();

        public MessagePump(IRepository repository, ICacheSourceInstance cache, IMessageProcessor processor, IMonik monik)
        {
            _repository = repository;
            _cache = cache;
            _processor = processor;
            _monik = monik;

            _monik.ApplicationVerbose("MessagePump created");
        }

        public void OnEmbeddedEvents(IEnumerable<Event> events)
        {
            foreach (var msg in events)
                _msgBuffer.Enqueue(msg);

            _newMessageEvent.Set();
        }

        private void OnProcessTask()
        {
            while (!_pumpCancellationTokenSource.IsCancellationRequested)
            {
                _newMessageEvent.WaitOne();

                Task.Delay(DelayOnProcess).Wait();

                try
                {
                    if (_msgBuffer.IsEmpty)
                        continue;

                    // TODO: use bulk insert and pk id generate in service !!!

                    while (_msgBuffer.TryDequeue(out Event msg))
                    {
                        var srcName = msg.Source;
                        var instName = msg.Instance;

                        if (srcName.Trim().Length != 0 && instName.Trim().Length != 0)
                        {
                            var instance = _cache.CheckSourceAndInstance(srcName, instName);
                            _processor.Process(msg, instance);
                        }
                        // TODO: increase count of ignored messages
                    }

                    _processor.FinalizeProcessing();
                }
                catch
                {
                    // TODO: trace and handle Repository problems...
                }
                finally
                {
                    _newMessageEvent.Reset();
                }
            }
        }

        public void OnStart()
        {
            // Create context for ActiveQueues
            var context = new ActiveQueueContext
            {
                OnError = (errorMessage) =>
                {
                    _monik.ApplicationError(errorMessage);
                    Thread.Sleep(DelayOnException);
                },
                OnVerbose = (verboseMessage) =>
                {
                    _monik.ApplicationVerbose(verboseMessage);
                },
                OnReceivedMessage = (msg) =>
                {
                    _msgBuffer.Enqueue(msg);
                    _newMessageEvent.Set();
                },
                OnReceivedMessages = (messages) =>
                {
                    foreach (var msg in messages)
                        _msgBuffer.Enqueue(msg);
                    _newMessageEvent.Set();
                }
            };

            // Load events sources
            var configs = _repository.GetEventSources();

            foreach (var it in configs)
            {
                try
                {
                    var queue = CreateActiveQueueByType(it.Type);

                    if (queue != null)
                    {
                        _queues.Add(queue);
                        queue.Start(it, context);
                    }
                    else
                        _monik.ApplicationWarning(
                            $"MessagePump.OnStart cannot initialize {it.Name}: unknown type {it.Type}");
                }
                catch (Exception ex)
                {
                    _monik.ApplicationError($"MessagePump.OnStart failed initialization {it.Name}: {ex.Message}");
                }
            }//configure all event sources

            // Start message processing
            _pumpTask = Task.Run(OnProcessTask);

            _monik.ApplicationVerbose("MessagePump started");
        }

        public void OnStop()
        {
            // 1. Stop all event sources
            foreach (var it in _queues)
                it.Stop();

            // 2. Flush buffers
            _newMessageEvent.Set();
            _pumpCancellationTokenSource.Cancel();

            _pumpTask?.Wait(WaitOnExit);
        }

        /// <summary>
        /// ActiveQueue Factory
        /// </summary>
        /// <param name="type"></param>
        /// <returns>New ActiveQueue instance based on the type parameter</returns>
        private static IActiveQueue CreateActiveQueueByType(EventQueueType type)
        {
            switch (type)
            {
                case EventQueueType.Azure:
                    return new AzureActiveQueue();

                case EventQueueType.Rabbit:
                    return new RabbitActiveQueue();

                case EventQueueType.Sql:
                    return new SqlActiveQueue();

                default:
                    return null;
            }
        }

    } //end class
}
