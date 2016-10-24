using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monik;
using Monik.Common;

namespace Monik.Service
{
  public interface IObject
  {
    void OnStart();
    void OnStop();
  }

  public interface IRepository
  {
    List<Source> GetAllSources();
    List<Instance> GetAllInstances();

    void CreateNewSource(Source aSrc);
    void CreateNewInstance(Instance aIns);

    long GetMaxLogID();
    long GetMaxKeepAliveID();

    List<Log_> GetLastLogs(int aTop);

    long? GetLogThreshold(int aDayDeep);
    long? GetKeepAliveThreshold(int aDayDeep);

    int CleanUpLog(long aLastLog);
    int CleanUpKeepAlive(long aLastKeepAlive);

    void CreateHourStat(DateTime aHour, long aLastLogID, long aLastKeepAliveID);

    void CreateKeepAlive(KeepAlive_ aKeepAlive);
    void CreateLog(Log_ aLog);

    List<EventQueue> GetEventSources();
  }

  public interface ISourceInstanceCache : IObject
  {
    Instance CheckSourceAndInstance(string aSourceName, string aInstanceName);
    Source GetSourceByInstanceID(int aInstanceID);
    Instance GetInstanceByID(int aInstanceID);
  }

  public interface ICacheLog : IObject
  {
    long LastLogID { get; }
    void OnNewLog(Log_ aLog);

    List<Log_> GetLogs(int? aTop, Order aOrder, long? aLastID, LogsFilter[] aFilters);
  }

  public interface ICacheKeepAlive : IObject
  {
    long LastKeepAliveID { get; }
    void OnNewKeepAlive(KeepAlive_ aKeepAlive);
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
