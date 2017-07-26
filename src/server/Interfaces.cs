using System;
using System.Collections.Generic;
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
		string CloudInstanceName { get; }

		string OutcomingConnectionString { get; }
		string OutcomingQueue { get; }
		int DayDeepLog { get; }
		int DayDeepKeepAlive { get; }
	}

	public interface IRepository
	{
		List<Source> GetAllSources();
		List<Instance> GetAllInstances();
		List<Group> GetAllGroupsAndFill();

		void CreateNewSource(Source aSrc);
		void CreateNewInstance(Instance aIns);
		void AddInstanceToGroup(Instance aIns, Group aGroup);

		long GetMaxLogId();
		long GetMaxKeepAliveId();

		List<Log_> GetLastLogs(int aTop);
		List<KeepAlive_> GetLastKeepAlive(int aTop);

		long? GetLogThreshold(int aDayDeep);
		long? GetKeepAliveThreshold(int aDayDeep);

		int CleanUpLog(long aLastLog);
		int CleanUpKeepAlive(long aLastKeepAlive);

		void CreateHourStat(DateTime aHour, long aLastLogId, long aLastKeepAliveId);

		void CreateKeepAlive(KeepAlive_ aKeepAlive);
		void CreateLog(Log_ aLog);

		List<EventQueue> GetEventSources();
	}

	public interface ISourceInstanceCache : IObject
	{
		Instance CheckSourceAndInstance(string aSourceName, string aInstanceName);
		Source GetSourceByInstanceId(int aInstanceId);
		Instance GetInstanceById(int aInstanceId);
		List<Instance> GetAllInstances();

		bool IsDefaultInstance(int aInstance);
		bool IsInstanceInGroup(int aInstanceId, short aGroupId);
	}

	public interface ICacheLog : IObject
	{
		long LastLogId { get; }
		void OnNewLog(Log_ aLog);

		List<Log_> GetLogs5(LogRequest aFilter);
	}

	public interface ICacheKeepAlive : IObject
	{
		long LastKeepAliveId { get; }
		void OnNewKeepAlive(KeepAlive_ aKeepAlive);

		List<KeepAlive_> GetKeepAlive(LogsFilter[] aFilters);
		List<KeepAlive_> GetKeepAlive2(KeepAliveRequest aFilter);
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
