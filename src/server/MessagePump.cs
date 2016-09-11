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

  public class MessagePump
  {
    private string FConnectionString;
    private SourceInstanceCache FCache;
    private MessageProcessor FProcessor;
    private List<ActiveQueue> FQueues;

    public MessagePump(string aConnectionString, SourceInstanceCache aCache, MessageProcessor aProcessor)
    {
      FConnectionString = aConnectionString;
      FCache = aCache;
      FProcessor = aProcessor;
      FQueues = null;

      Initialize();
    }

    private void Initialize()
    {
      if (FQueues != null)
        foreach (var it in FQueues)
          it.Queue.Close();

      FQueues = new List<ActiveQueue>();
      var _configs = SimpleCommand.ExecuteQuery<EventQueue>(FConnectionString, "select * from [mon].[EventQueue]");
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

            var _sourceInstance = FCache.CheckSourceAndInstance(Helper.Utf8ToUtf16(_msg.Source), Helper.Utf8ToUtf16(_msg.Instance));
            FProcessor.Process(_msg, _sourceInstance);
          }
          catch (Exception _e)
          {
            M.ApplicationError("Message processing error: " + _e.Message);
          }
        });

        FQueues.Add(_queue);
      }
    }
    
  }//end class
}
