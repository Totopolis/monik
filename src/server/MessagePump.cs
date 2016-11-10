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

namespace Monik.Service
{
  public class ActiveQueue
  {
    public EventQueue Config { get; set; }
    public QueueClient Queue { get; set; }
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
        ActiveQueue _queue = new ActiveQueue()
        {
          Config = it,
          Queue = QueueClient.CreateFromConnectionString(it.ConnectionString, it.QueueName)
        };

        _queue.Queue.OnMessage(message =>
        {
          try
          {
            byte[] _buf = message.GetBody<byte[]>();
            Event _msg = Event.Parser.ParseFrom(_buf);
            
            var _instance = FCache.CheckSourceAndInstance(Helper.Utf8ToUtf16(_msg.Source), Helper.Utf8ToUtf16(_msg.Instance));
            FProcessor.Process(_msg, _instance);
          }
          catch (Exception _e)
          {
            FControl.ApplicationError("MessagePump.OnMessage: {0}", _e.Message);
          }
        });

        FQueues.Add(_queue);
      }//foreach config

      FControl.ApplicationVerbose("MessagePump started");
    }

    public void OnStop()
    {
      if (FQueues != null)
        foreach (var it in FQueues)
          it.Queue.Close();
    }
    
  }//end class
}
