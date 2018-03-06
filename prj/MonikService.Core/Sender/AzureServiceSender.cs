using Monik.Client;
using Monik.Service;

namespace MonikService.Core.Core
{
	public class AzureServiceSender : AzureSender
	{
		public AzureServiceSender(IServiceSettings aSettings) : base(aSettings.OutcomingConnectionString, aSettings.OutcomingQueue)
		{
		}
	} //end of class
}
