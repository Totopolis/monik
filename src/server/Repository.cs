using Gerakul.FastSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monik;
using Monik.Common;
using System.Diagnostics;

namespace Monik.Service
{
  public class Repository : IRepository
  {
    public static string ConnectionString = string.Empty;

    public Repository() { }

    public List<Source> GetAllSources()
    {
      return SimpleCommand.ExecuteQuery<Source>(ConnectionString, "select * from [mon].[Source]").ToList();
    }

    public List<Instance> GetAllInstances()
    {
      return SimpleCommand.ExecuteQuery<Instance>(ConnectionString, "select * from [mon].[Instance]").ToList();
    }

    public void CreateNewSource(Source aSrc)
    {
      aSrc.ID = (short)MappedCommand.InsertAndGetId<Source>(ConnectionString, "[mon].[Source]", aSrc, "ID");
    }

    public void CreateNewInstance(Instance aIns)
    {
      aIns.ID = (int)MappedCommand.InsertAndGetId<Instance>(ConnectionString, "[mon].[Instance]", aIns, "ID");
    }

    public long GetMaxLogID()
    {
      var _res = SimpleCommand.ExecuteScalar(ConnectionString, "select max(ID) from [mon].[Log]");
      return _res == System.DBNull.Value ? 0 : (long)_res;
    }

    public long GetMaxKeepAliveID()
    {
      var _res = SimpleCommand.ExecuteScalar(ConnectionString, "select max(ID) from [mon].[KeepAlive]");
      return _res == System.DBNull.Value ? 0 : (long)_res;
    }

    public List<Log_> GetLastLogs(int aTop)
    {
      return SimpleCommand.ExecuteQuery<Log_>(ConnectionString, $"select top {aTop} * from [mon].[Log] order by ID desc").ToList();
    }

    public long? GetLogThreshold(int aDayDeep)
    {
      var _logThreshold = SimpleCommand.ExecuteScalar(ConnectionString, "select max(LastLogID) from mon.HourStat where Hour < DATEADD(DAY, -@p0, GETDATE())", aDayDeep);
      return _logThreshold == System.DBNull.Value ? null : (long?)_logThreshold;
    }

    public long? GetKeepAliveThreshold(int aDayDeep)
    {
      var _kaThreshold = SimpleCommand.ExecuteScalar(ConnectionString, "select max(LastKeepAliveID) from mon.HourStat where Hour < DATEADD(DAY, -@p0, GETDATE())", aDayDeep);
      return _kaThreshold == System.DBNull.Value ? null : (long?)_kaThreshold;
    }

    public int CleanUpLog(long aLastLog)
    {
      return SimpleCommand.ExecuteNonQuery(ConnectionString, "delete from mon.Log where ID < @p0", aLastLog);
    }

    public int CleanUpKeepAlive(long aLastKeepAlive)
    {
      return SimpleCommand.ExecuteNonQuery(ConnectionString, "delete from mon.KeepAlive where ID < @p0", aLastKeepAlive);
    }

    public void CreateHourStat(DateTime aHour, long aLastLogID, long aLastKeepAliveID)
    {
      var stat = new { Hour = aHour, LastLogID = aLastLogID, LastKeepAliveID = aLastKeepAliveID };
      MappedCommand.Insert(ConnectionString, "[mon].[HourStat]", stat);
    }

    public void CreateKeepAlive(KeepAlive_ aKeepAlive)
    {
      aKeepAlive.ID = (long)MappedCommand.InsertAndGetId<KeepAlive_>(ConnectionString, "[mon].[KeepAlive]", aKeepAlive, "ID");
    }

    public void CreateLog(Log_ aLog)
    {
      aLog.ID = (long)MappedCommand.InsertAndGetId<Log_>(ConnectionString, "[mon].[Log]", aLog, "ID");
    }

    public List<EventQueue> GetEventSources()
    {
      return SimpleCommand.ExecuteQuery<EventQueue>(ConnectionString, "select * from [mon].[EventQueue]").ToList();
    }
  }//end of class
}
