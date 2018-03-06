using Monik.Common;

namespace Monik.Service
{
	public interface IObject
	{
		void OnStart();
		void OnStop();
	}

    public interface IMessagePump : IObject
	{

	}

	public interface IMessageProcessor : IObject
	{
		void Process(Event aEvent, Instance aInstance);
	}

	public interface IWebService : IObject
	{

	}
}
