using Monik.Client.Azure.Sender;
using MonikService.Core.Settings;

namespace MonikService.Core.Sender
{
    public class AzureServiceSender : AzureSender
    {
        public AzureServiceSender(IServiceSettings aSettings) : base(aSettings.OutcomingConnectionString,
                                                                     aSettings.OutcomingQueue) { }
    } //end of class
}