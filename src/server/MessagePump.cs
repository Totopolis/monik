using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Monik.Common;
using EasyNetQ;
using System.Threading;

namespace Monik.Service
{
    public class ActiveQueue
    {
        public EventQueue Config { get; set; }
        public QueueClient AzureQueue { get; set; }
        public IAdvancedBus RabbitQueue { get; set; }
    }

    public class MessagePump : IMessagePump
    {
        private const int DelayOnException = 1000; //in ms
        private const int DelayOnProcess = 1000; //in ms

        private readonly IRepository _repository;
        private readonly ISourceInstanceCache _cache;
        private readonly IMessageProcessor _processor;
        private readonly IMonik _monik;

        private List<ActiveQueue> _queues = new List<ActiveQueue>();

        private ConcurrentQueue<Event> _msgBuffer = new ConcurrentQueue<Event>();

        private readonly Task _pumpTask;
        private readonly ManualResetEvent _newMessageEvent = new ManualResetEvent(false);
        private readonly CancellationTokenSource _pumpCancellationTokenSource = new CancellationTokenSource();

        public MessagePump(IRepository repository, ISourceInstanceCache cache, IMessageProcessor processor, IMonik monik)
        {
            _repository = repository;
            _cache = cache;
            _processor = processor;
            _monik = monik;

            _pumpTask = Task.Run(() => { OnProcessTask(); });

            _monik.ApplicationVerbose("MessagePump created");
        }

        public void OnEmbeddedEvents(ConcurrentQueue<Event> events)
        {
            while (events.TryDequeue(out Event msg))
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
                            var instance = _cache.CheckSourceAndInstance(Helper.Utf8ToUtf16(srcName), Helper.Utf8ToUtf16(instName));
                            _processor.Process(msg, instance);
                        }
                        // TODO: increase count of ignored messages
                    }
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
            // Load events sources
            var configs = _repository.GetEventSources();

            foreach (var it in configs)
            {
                try
                {
                    ActiveQueue queue = new ActiveQueue()
                    {
                        Config = it,
                        AzureQueue = null,
                        RabbitQueue = null
                    };

                    if (it.Type == 1)
                        InitializeServiceBus(queue);
                    else if (it.Type == 2)
                        InitializeRabbitMq(queue);

                    _queues.Add(queue);
                }
                catch (Exception ex)
                {
                    _monik.ApplicationError($"MessagePump.OnStart failed initialization {it.Name}: {ex.Message}");
                }
            }//configure all event sources

            _monik.ApplicationVerbose("MessagePump started");
        }

        private void InitializeServiceBus(ActiveQueue active)
        {
            active.AzureQueue = QueueClient.CreateFromConnectionString(active.Config.ConnectionString, active.Config.QueueName);

            active.AzureQueue.OnMessage(message =>
            {
                try
                {
                    byte[] buf = message.GetBody<byte[]>();
                    Event msg = Event.Parser.ParseFrom(buf);

                    _msgBuffer.Enqueue(msg);
                    _newMessageEvent.Set();
                }
                catch (Exception ex)
                {
                    _monik.ApplicationError($"MessagePump.OnMessage ServiceBus Parse Error: {ex.Message}");
                    System.Threading.Thread.Sleep(DelayOnException);
                }
            });
        }

        private void InitializeRabbitMq(ActiveQueue active)
        {
            active.RabbitQueue = RabbitHutch.CreateBus(active.Config.ConnectionString).Advanced;
            var queue = active.RabbitQueue.QueueDeclare(active.Config.QueueName);

            active.RabbitQueue.Consume(queue, (body, properties, info) => Task.Factory.StartNew(() =>
            {
                try
                {
                    Event msg = Event.Parser.ParseFrom(body);

                    _msgBuffer.Enqueue(msg);
                    _newMessageEvent.Set();
                }
                catch (Exception ex)
                {
                    _monik.ApplicationError($"MessagePump.OnMessage RabbitMQ Parse Error: {ex.Message}");
                    System.Threading.Thread.Sleep(DelayOnException);
                }
            }));
        }

        public void OnStop()
        {
            // 1. Stop all event sources
            foreach (var it in _queues)
                if (it.Config.Type == 1)
                    it.AzureQueue.Close();
                else if (it.Config.Type == 2)
                    it.RabbitQueue.Dispose();

            // 2. Flsuh buffers
            _newMessageEvent.Set();
            _pumpCancellationTokenSource.Cancel();

            Task.Delay(2000).Wait(); // TODO: is it correct?
        }

    } //end class
}
