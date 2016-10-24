using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monik;
using Monik.Common;
using Monik.Client;

namespace Monik.Service
{
  public class CacheLog : ICacheLog
  {
    private IRepository FRepository;
    private List<Log_> FLogs;
    private ISourceInstanceCache FCache;
    private long FOldestID;

    public CacheLog(IRepository aRepository, ISourceInstanceCache aCache)
    {
      FRepository = aRepository;
      FLogs = null;
      FCache = aCache;
      FOldestID = 0;
      M.ApplicationInfo("CacheLog created");
    }

    public void OnStart()
    {
      // load from database

      // 1. last IDs
      LastLogID = FRepository.GetMaxLogID();

      // 2. load top logs
      FLogs = FRepository.GetLastLogs(1000);

      FOldestID = FLogs.Min(lg => lg.ID);
    }

    public void OnStop()
    {
      // nothing
    }

    public long LastLogID { get; private set; }

    public void OnNewLog(Log_ aLog)
    {
      LastLogID = aLog.ID;

      lock (this)
      {
        FLogs.Insert(0, aLog);
        LastLogID = aLog.ID;
      }

      // TODO: pop overhead logs
    }

    private bool IsFiltered(Log_ aLog, LogsFilter[] aFilters)
    {
      foreach (var it in aFilters)
      {
        bool _result = true;

        if (it.FilterBySource)
        {
          Source _src = FCache.GetSourceByInstanceID(aLog.InstanceID);
          if (_src == null)
            return false;

          _result &= _src.ID == it.SourceID;
        }

        if (it.FilterByInstance)
          _result &= it.InstanceID == aLog.ID;

        if (it.FilterByLevel)
          _result &= it.Level == aLog.Level;

        if (it.FilterBySeverity)
          _result &= it.Severity == aLog.Severity;

        if (_result)
          return true;
      }//foreach

      return false;
    }

    public List<Log_> GetLogs(int? aTop, Order aOrder, long? aLastID, LogsFilter[] aFilters)
    {
      List<Log_> _res = new List<Log_>();

      if (aLastID.HasValue)
        if (aLastID.Value < FOldestID)
          return _res;
        else
        {
          lock (this)
            _res = FLogs.FindAll(lg => lg.ID > aLastID.Value).ToList();

          _res = (aOrder == Order.Desc ? _res.OrderByDescending(lg => lg.ID) : _res.OrderBy(lg => lg.ID)).ToList();
        }
      else
        lock (this)
          _res = (aOrder == Order.Desc ? FLogs.OrderByDescending(lg => lg.ID) : FLogs.OrderBy(lg => lg.ID)).ToList();

      if (aFilters != null && aFilters.Length > 0)
        _res = _res.FindAll(lg => IsFiltered(lg, aFilters)).ToList();

      if (aTop.HasValue)
        _res = _res.Take(aTop.Value).ToList();

      return _res;
    }

  }//end of class
}
