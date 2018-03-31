﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Monik.Common;
using EasyNetQ;

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
        public const int DelayOnException = 1000; //in ms

        private readonly IRepository _repository;
        private readonly ISourceInstanceCache _cache;
        private readonly IMessageProcessor _processor;
        private readonly IMonik _monik;

        private List<ActiveQueue> _queues;

        public MessagePump(IRepository aRepository, ISourceInstanceCache aCache, IMessageProcessor aProcessor,
            IMonik monik)
        {
            _repository = aRepository;
            _cache = aCache;
            _processor = aProcessor;
            _monik = monik;
            _queues = null;

            _monik.ApplicationVerbose("MessagePump created");
        }

        public void OnStart()
        {
            // Load events sources
            _queues = new List<ActiveQueue>();
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
            }

            _monik.ApplicationVerbose("MessagePump started");
        }

        private void InitializeServiceBus(ActiveQueue aActive)
        {
            aActive.AzureQueue = QueueClient.CreateFromConnectionString(aActive.Config.ConnectionString, aActive.Config.QueueName);

            aActive.AzureQueue.OnMessage(message =>
            {
                try
                {
                    byte[] buf = message.GetBody<byte[]>();
                    Event msg = Event.Parser.ParseFrom(buf);

                    if (msg.Source.Trim().Length != 0 && msg.Instance.Trim().Length != 0)
                    {
                        var instance = _cache.CheckSourceAndInstance(Helper.Utf8ToUtf16(msg.Source), Helper.Utf8ToUtf16(msg.Instance));
                        _processor.Process(msg, instance);
                    }
                    // TODO: else increase ignored counter
                }
                catch (Exception ex)
                {
                    _monik.ApplicationError($"MessagePump.OnMessage ServiceBus: {ex.Message}");
                    System.Threading.Thread.Sleep(DelayOnException);
                }
            });
        }

        private void InitializeRabbitMq(ActiveQueue aActive)
        {
            aActive.RabbitQueue = RabbitHutch.CreateBus(aActive.Config.ConnectionString).Advanced;

            // https://github.com/EasyNetQ/EasyNetQ/wiki/the-advanced-api

            var queue = aActive.RabbitQueue.QueueDeclare(aActive.Config.QueueName);

            aActive.RabbitQueue.Consume(queue, (body, properties, info) => Task.Factory.StartNew(() =>
            {
                try
                {
                    Event msg = Event.Parser.ParseFrom(body);

                    if (msg.Source.Trim().Length != 0 && msg.Instance.Trim().Length != 0)
                    {
                        var instance = _cache.CheckSourceAndInstance(Helper.Utf8ToUtf16(msg.Source), Helper.Utf8ToUtf16(msg.Instance));
                        _processor.Process(msg, instance);
                    }
                    // TODO: else increase ignored counter
                }
                catch (Exception ex)
                {
                    _monik.ApplicationError($"MessagePump.OnMessage RabbitMQ: {ex.Message}");
                    System.Threading.Thread.Sleep(DelayOnException);
                }
            }));
        }

        public void OnStop()
        {
            if (_queues != null)
                foreach (var it in _queues)
                    if (it.Config.Type == 1)
                        it.AzureQueue.Close();
                    else if (it.Config.Type == 2)
                        it.RabbitQueue.Dispose();
        }

    } //end class
}
