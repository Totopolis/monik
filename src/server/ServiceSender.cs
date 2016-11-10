using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monik;
using Monik.Common;
using Monik.Client;
using System.Collections.Concurrent;

namespace Monik.Service
{
  public class ServiceSender : IClientSender
  {
    private IServiceSettings FServiceSettings;
    private IClientSender FSender;

    public ServiceSender(IServiceSettings aSettings)
    {
      FServiceSettings = aSettings;

      FSender = new AzureSender(aSettings.OutcomingConnectionString, aSettings.OutcomingQueue);
    }

    public void SendMessages(ConcurrentQueue<Event> aQueue)
    {
      FSender.SendMessages(aQueue);
    }
  }//end of class
}
