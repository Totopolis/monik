using Gerakul.FastSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monik;
using Monik.Common;
using System.Diagnostics;
using Microsoft.ServiceBus.Messaging;
using Monik.Client;
using EasyNetQ;
using EasyNetQ.Topology;

namespace Monik.Service
{
  public class ActiveQueue
  {
    public EventQueue Config { get; set; }
    public QueueClient AzureQueue { get; set; }
    public IAdvancedBus RabbitQueue { get; set; }
  }

  public class EventQueue
  {
    public int ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public byte Type { get; set; }
    public string ConnectionString { get; set; }
    public string QueueName { get; set; }
  }

  public class MessagePump : IMessagePump
  {
    private IRepository FRepository;
    private ISourceInstanceCache FCache;
    private IMessageProcessor FProcessor;
    private IClientControl FControl;

    private List<ActiveQueue> FQueues;

    public MessagePump(IRepository aRepository, ISourceInstanceCache aCache, IMessageProcessor aProcessor, IClientControl aControl)
    {
      FRepository = aRepository;
      FCache = aCache;
      FProcessor = aProcessor;
      FControl = aControl;
      FQueues = null;

      FControl.ApplicationVerbose("MessagePump created");
    }

    public void OnStart()
    {
      FQueues = new List<ActiveQueue>();
      var _configs = FRepository.GetEventSources();

      foreach (var it in _configs)
      {
        try
        {
          ActiveQueue _queue = new ActiveQueue()
          {
            Config = it,
            AzureQueue = null,
            RabbitQueue = null
          };

          if (it.Type == 1)
            InitializeServiceBus(_queue);
          else
            if (it.Type == 2)
            InitializeRabbitMQ(_queue);

          FQueues.Add(_queue);
        }
        catch(Exception _e)
        {
          FControl.ApplicationError($"MessagePump.OnStart failed initialization {it.Name}: {_e.Message}");
        }
      }

      FControl.ApplicationVerbose("MessagePump started");
    }

    private void InitializeServiceBus(ActiveQueue aActive)
    {
      aActive.AzureQueue = QueueClient.CreateFromConnectionString(aActive.Config.ConnectionString, aActive.Config.QueueName);

      aActive.AzureQueue.OnMessage(message =>
      {
        try
        {
          byte[] _buf = message.GetBody<byte[]>();
          Event _msg = Event.Parser.ParseFrom(_buf);

          if (_msg.Source.Trim().Length != 0 && _msg.Instance.Trim().Length != 0)
          {
            var _instance = FCache.CheckSourceAndInstance(Helper.Utf8ToUtf16(_msg.Source), Helper.Utf8ToUtf16(_msg.Instance));
            FProcessor.Process(_msg, _instance);
          }
          // TODO: else increase ignored counter
        }
        catch (Exception _e)
        {
          FControl.ApplicationError($"MessagePump.OnMessage ServiceBus: {_e.Message}");
        }
      });
    }

    private void InitializeRabbitMQ(ActiveQueue aActive)
    {
      aActive.RabbitQueue = RabbitHutch.CreateBus(aActive.Config.ConnectionString).Advanced;
      
      // https://github.com/EasyNetQ/EasyNetQ/wiki/the-advanced-api

      var queue = aActive.RabbitQueue.QueueDeclare(aActive.Config.QueueName);

      aActive.RabbitQueue.Consume(queue, (body, properties, info) => Task.Factory.StartNew(() =>
      {
        try
        {
          Event _msg = Event.Parser.ParseFrom(body);

          if (_msg.Source.Trim().Length != 0 && _msg.Instance.Trim().Length != 0)
          {
            var _instance = FCache.CheckSourceAndInstance(Helper.Utf8ToUtf16(_msg.Source), Helper.Utf8ToUtf16(_msg.Instance));
            FProcessor.Process(_msg, _instance);
          }
          // TODO: else increase ignored counter
        }
        catch (Exception _e)
        {
          FControl.ApplicationError($"MessagePump.OnMessage RabbitMQ: {_e.Message}");
        }
      }));
    }

    public void OnStop()
    {
      if (FQueues != null)
        foreach (var it in FQueues)
          if (it.Config.Type == 1)
            it.AzureQueue.Close();
          else
            if (it.Config.Type == 2)
            it.RabbitQueue.Dispose();
    }
    
  }//end class
}
