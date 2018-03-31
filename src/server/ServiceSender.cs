using Monik.Common;
using Monik.Client;
using System.Collections.Concurrent;

namespace Monik.Service
{
    public class ServiceSender : IClientSender
    {
        private readonly IServiceSettings _serviceSettings;
        private readonly IClientSender _sender;

        public ServiceSender(IServiceSettings aSettings)
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
