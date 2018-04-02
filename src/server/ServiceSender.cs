using Monik.Common;
using Monik.Client;
using System.Collections.Concurrent;

namespace Monik.Service
{
    public class ServiceSender : IMonikSender
    {
        private readonly IMonikServiceSettings _serviceSettings;
        private readonly IMonikSender _sender;

        public ServiceSender(IMonikServiceSettings aSettings)
        {
            _serviceSettings = aSettings;
            _sender = new AzureSender(aSettings.OutcomingConnectionString, aSettings.OutcomingQueue);
        }

        public void SendMessages(ConcurrentQueue<Event> aQueue)
        {
            _sender.SendMessages(aQueue);
        }
    } //end of class
}
