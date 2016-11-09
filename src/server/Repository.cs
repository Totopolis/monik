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
    private IServiceSettings FSettings;

    public Repository(IServiceSettings aSettings)
    {
      FSettings = aSettings;
    }

    public List<Source> GetAllSources()
    {
      return SimpleCommand.ExecuteQuery<Source>(FSettings.DBConnectionString, "select * from [mon].[Source]").ToList();
    }

    public List<Instance> GetAllInstances()
    {
      return SimpleCommand.ExecuteQuery<Instance>(FSettings.DBConnectionString, "select * from [mon].[Instance]").ToList();
    }

    public void CreateNewSource(Source aSrc)
    {
      aSrc.ID = (short)MappedCommand.InsertAndGetId<Source>(FSettings.DBConnectionString, "[mon].[Source]", aSrc, "ID");
    }

    public void CreateNewInstance(Instance aIns)
    {
      aIns.ID = (int)MappedCommand.InsertAndGetId<Instance>(FSettings.DBConnectionString, "[mon].[Instance]", aIns, "ID");
    }

    public long GetMaxLogID()
    {
      var _res = SimpleCommand.ExecuteScalar(FSettings.DBConnectionString, "select max(ID) from [mon].[Log]");
      return _res == System.DBNull.Value ? 0 : (long)_res;
    }

    public long GetMaxKeepAliveID()
    {
      var _res = SimpleCommand.ExecuteScalar(FSettings.DBConnectionString, "select max(ID) from [mon].[KeepAlive]");
      return _res == System.DBNull.Value ? 0 : (long)_res;
    }

    public List<Log_> GetLastLogs(int aTop)
    {
      return SimpleCommand.ExecuteQuery<Log_>(FSettings.DBConnectionString, $"select top {aTop} * from [mon].[Log] order by ID desc").ToList();
    }

    public List<KeepAlive_> GetLastKeepAlive(int aTop)
    {
      return SimpleCommand.ExecuteQuery<KeepAlive_>(FSettings.DBConnectionString, $"select top {aTop} * from [mon].[KeepAlive] order by ID desc").ToList();
    }

    public long? GetLogThreshold(int aDayDeep)
    {
      var _logThreshold = SimpleCommand.ExecuteScalar(FSettings.DBConnectionString, "select max(LastLogID) from mon.HourStat where Hour < DATEADD(DAY, -@p0, GETDATE())", aDayDeep);
      return _logThreshold == System.DBNull.Value ? null : (long?)_logThreshold;
    }

    public long? GetKeepAliveThreshold(int aDayDeep)
    {
      var _kaThreshold = SimpleCommand.ExecuteScalar(FSettings.DBConnectionString, "select max(LastKeepAliveID) from mon.HourStat where Hour < DATEADD(DAY, -@p0, GETDATE())", aDayDeep);
      return _kaThreshold == System.DBNull.Value ? null : (long?)_kaThreshold;
    }

    public int CleanUpLog(long aLastLog)
    {
      return SimpleCommand.ExecuteNonQuery(FSettings.DBConnectionString, "delete from mon.Log where ID < @p0", aLastLog);
    }

    public int CleanUpKeepAlive(long aLastKeepAlive)
    {
      return SimpleCommand.ExecuteNonQuery(FSettings.DBConnectionString, "delete from mon.KeepAlive where ID < @p0", aLastKeepAlive);
    }

    public void CreateHourStat(DateTime aHour, long aLastLogID, long aLastKeepAliveID)
    {
      var stat = new { Hour = aHour, LastLogID = aLastLogID, LastKeepAliveID = aLastKeepAliveID };
      MappedCommand.Insert(FSettings.DBConnectionString, "[mon].[HourStat]", stat);
    }

    public void CreateKeepAlive(KeepAlive_ aKeepAlive)
    {
      aKeepAlive.ID = (long)MappedCommand.InsertAndGetId<KeepAlive_>(FSettings.DBConnectionString, "[mon].[KeepAlive]", aKeepAlive, "ID");
    }

    public void CreateLog(Log_ aLog)
    {
      aLog.ID = (long)MappedCommand.InsertAndGetId<Log_>(FSettings.DBConnectionString, "[mon].[Log]", aLog, "ID");
    }

    public List<EventQueue> GetEventSources()
    {
      return SimpleCommand.ExecuteQuery<EventQueue>(FSettings.DBConnectionString, "select * from [mon].[EventQueue]").ToList();
    }
  }//end of class
}
