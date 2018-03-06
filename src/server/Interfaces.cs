using Monik.Common;

namespace Monik.Service
{
	public interface IObject
	{
		void OnStart();
		void OnStop();
	}

	public interface IServiceSettings : IObject
	{
		string DbConnectionString { get; }
		string InstanceName { get; }

		string OutcomingConnectionString { get; }
		string OutcomingQueue { get; }
		int DayDeepLog { get; }
		int DayDeepKeepAlive { get; }
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
