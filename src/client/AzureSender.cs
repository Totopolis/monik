using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.ServiceBus.Messaging;
using System.IO;
using System.Threading;
using Monik.Common;

namespace Monik.Client
{
  public class AzureSender : IClientSender
  {
    private string FServiceBusConnectionString;
    private string FQueueName;

    public AzureSender(string aServiceBusConnectionString, string aQueueName)
    {
      FServiceBusConnectionString = aServiceBusConnectionString;
      FQueueName = aQueueName;
    }

    public void SendMessages(ConcurrentQueue<Event> aQueue)
    {
      var _client = QueueClient.CreateFromConnectionString(FServiceBusConnectionString, FQueueName);
      Event _msg;

      try
      {
        while (aQueue.TryDequeue(out _msg))
        {
          var _arr = _msg.ToByteArray();
          var _message = new BrokeredMessage(_arr);
          _client.Send(_message);
        }
      }
      finally { _client.Close(); }
    }
  }
}
