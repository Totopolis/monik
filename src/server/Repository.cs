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
  public class WebServiceSourcesResponse
  {
    public int ID { get; set; }
    public DateTime Created { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
  }

  public class WebServiceInstancesResponse
  {
    public int ID { get; set; }
    public DateTime Created { get; set; }
    public int SourceID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
  }

  public class WebServiceLogResponse
  {
    public long ID { get; set; }
    public int InstanceID { get; set; }
    public DateTime Created { get; set; }
    public int Level { get; set; }
    public int Severity { get; set; }
    public string Body { get; set; }
  }

  public class LogsFilter
  {
    public bool FilterBySource { get; set; }
    public int SourceID { get; set; }

    public bool FilterByInstance { get; set; }
    public int InstanceID { get; set; }

    public bool FilterByLevel { get; set; }
    public int Level { get; set; }

    public bool FilterBySeverity { get; set; }
    public int Severity { get; set; }
  }

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
      aKeepAlive.ID = (int)MappedCommand.InsertAndGetId<KeepAlive_>(ConnectionString, "[mon].[KeepAlive]", aKeepAlive, "ID");
    }

    public void CreateLog(Log_ aLog)
    {
      aLog.ID = (int)MappedCommand.InsertAndGetId<Log_>(ConnectionString, "[mon].[Log]", aLog, "ID");
    }

    public List<EventQueue> GetEventSources()
    {
      return SimpleCommand.ExecuteQuery<EventQueue>(ConnectionString, "select * from [mon].[EventQueue]").ToList();
    }

    public List<WebServiceSourcesResponse> GetSources()
    {
      string _query = @"select * from [mon].[Source]";

      var q1 = SimpleCommand.ExecuteQuery<WebServiceSourcesResponse>(ConnectionString, _query);
      return q1.ToList();
    }

    public List<WebServiceInstancesResponse> GetInstances()
    {
      string _query = @"select * from [mon].[Instance]";

      var q1 = SimpleCommand.ExecuteQuery<WebServiceInstancesResponse>(ConnectionString, _query);
      return q1.ToList();
    }

    private string GetWhereClause(LogsFilter aFilter)
    {
      if (!aFilter.FilterByInstance && !aFilter.FilterBySource && !aFilter.FilterByLevel && !aFilter.FilterBySeverity)
        return "";

      string _res = "( 1=1 ";
      _res += aFilter.FilterByInstance ? " and ins.ID = " + aFilter.InstanceID : "";
      _res += aFilter.FilterBySource ? " and src.ID = " + aFilter.SourceID : "";
      _res += aFilter.FilterByLevel ? " and lg.Level = " + aFilter.Level : "";
      _res += aFilter.FilterBySeverity ? " and lg.Severity = " + aFilter.Severity : "";
      _res += ")";

      return _res;
    }

    public List<WebServiceLogResponse> GetFilteredLogs2(int? aTop, string aOrder, long? aLastID, LogsFilter[] aFilters)
    {
      string _whereClause = "";

      string _lastClouse = aLastID.HasValue ? " lg.ID > " + aLastID.Value : " ";

      if (aFilters.Length == 0)
        _whereClause = aLastID.HasValue ? "where " + _lastClouse : "";
      else
      if (aFilters.Length == 1)
        _whereClause = "where " + GetWhereClause(aFilters[0]) + (aLastID.HasValue ? " and " + _lastClouse : "");
      else
        if (aFilters.Length > 1)
      {
        _whereClause = "where (" + GetWhereClause(aFilters[0]);
        for (int i = 1; i < aFilters.Length - 1; i++)
          _whereClause += " or " + GetWhereClause(aFilters[i]);
        _whereClause += ") " + (aLastID.HasValue ? "and " + _lastClouse : "");
      }

      string _orderClouse = aOrder == "desc" ? "order by lg.ID desc" : "order by lg.ID asc";

      string _query = "select top " + aTop + @" lg.ID, ins.ID as InstanceID, lg.Created, lg.Level, lg.Severity, lg.Body
  from mon.Log lg
  join mon.Instance ins on lg.InstanceID = ins.ID
  join mon.Source src on src.ID = ins.SourceID 
  " +
  _whereClause + Environment.NewLine + _orderClouse;

      var q1 = SimpleCommand.ExecuteQuery<WebServiceLogResponse>(ConnectionString, _query);
      return q1.ToList();
    }

    public List<WebServiceLogResponse> GetFilteredLogs(int aTop, LogsFilter[] aFilters)
    {
      string _whereClause = "";

      if (aFilters.Length == 1)
        _whereClause = "where " + GetWhereClause(aFilters[0]);
      else
        if (aFilters.Length > 1)
      {
        _whereClause = "where " + GetWhereClause(aFilters[0]);
        for (int i = 1; i < aFilters.Length - 1; i++)
          _whereClause += " or " + GetWhereClause(aFilters[i]);
      }

      string _query = "select top " + aTop + @"lg.ID, ins.ID as InstanceID, lg.Created, lg.Level, lg.Severity, lg.Body
  from mon.Log lg
  join mon.Instance ins on lg.InstanceID = ins.ID
  join mon.Source src on src.ID = ins.SourceID 
  " +
  _whereClause + @"
  order by ID desc";

      var q1 = SimpleCommand.ExecuteQuery<WebServiceLogResponse>(ConnectionString, _query);
      return q1.ToList();
    }

  }//end of class
}
