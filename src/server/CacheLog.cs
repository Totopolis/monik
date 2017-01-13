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
    private IClientControl FControl;

    private List<Log_> FLogs;
    private ISourceInstanceCache FCache;
    private long FOldestID;

    public CacheLog(IRepository aRepository, ISourceInstanceCache aCache, IClientControl aControl)
    {
      FRepository = aRepository;
      FControl = aControl;

      FLogs = null;
      FCache = aCache;
      FOldestID = 0;

      FControl.ApplicationVerbose("CacheLog created");
    }

    public void OnStart()
    {
      // load from database

      // 1. last IDs
      LastLogID = FRepository.GetMaxLogID();

      // 2. load top logs
      FLogs = FRepository.GetLastLogs(1000);

      FOldestID = FLogs.Count == 0 ? 0 : FLogs.Min(lg => lg.ID);

      FControl.ApplicationVerbose("CacheLog started");
    }

    public void OnStop()
    {
      // nothing
    }

    public long LastLogID { get; private set; }

    public void OnNewLog(Log_ aLog)
    {
      lock (this)
      {
        FLogs.Add(aLog);
        LastLogID = aLog.ID;
      }

      // TODO: pop overhead logs
    }

    public List<Log_> GetLogs4(int? aGroup, long? aLastID, int? aSeverityCutoff, int? aLevel, int? aTop)
    {
      throw new NotImplementedException();
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

        if (it.EnableCutoffSeverity)
          _result &= it.CutoffSeverity >= aLog.Severity;

        if (_result)
          return true;
      }//foreach

      return false;
    }

    public List<Log_> GetLogs(int? aTop, Order aOrder, long? aLastID, LogsFilter[] aFilters)
    {
      lock (this)
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

        // TODO: stop when top reach
        if (aFilters != null && aFilters.Length > 0)
          _res = _res.FindAll(lg => IsFiltered(lg, aFilters)).ToList();

        // remove not default groups
        _res.RemoveAll(lg => !FCache.IsDefaultInstance(lg.InstanceID));

        if (aTop.HasValue)
          _res = _res.Take(aTop.Value).ToList();

        return _res;
      } // TODO: optimize
    }

    private bool IsFiltered5(Log_ aLog, LogRequest aFilter)
    {
      if (aFilter.SeverityCutoff.HasValue && aLog.Severity > aFilter.SeverityCutoff.Value)
        return false;

      if (aFilter.Level.HasValue && aLog.Level != aFilter.Level.Value)
        return false;

      bool _groupsEmpty = aFilter.Groups == null || aFilter.Groups.Length == 0;
      bool _instancesEmpty = aFilter.Instances == null || aFilter.Instances.Length == 0;

      if (_groupsEmpty && _instancesEmpty && FCache.IsDefaultInstance(aLog.InstanceID))
        return true;

      bool _instanceIn = _instancesEmpty ?
        false :
        aFilter.Instances.Contains(aLog.InstanceID);

      bool _groupIn = _groupsEmpty ?
        false :
        aFilter.Groups.Where(x => FCache.IsInstanceInGroup(aLog.InstanceID, x)).Count() > 0;

      return _instanceIn | _groupIn;
    }

    public List<Log_> GetLogs5(LogRequest aFilter)
    {
      List<Log_> _res = new List<Log_>();

      if (aFilter == null)
        return _res;

      if (aFilter.LastID.HasValue && aFilter.LastID.Value < FOldestID)
        return _res;

      lock (this)
      {
        _res = aFilter.LastID.HasValue ?
          FLogs.FindAll(lg => lg.ID > aFilter.LastID.Value).ToList() :
          FLogs.Select(x => x).ToList();
      }

      // TODO: remove magic number
      int _top = aFilter.Top.HasValue ? aFilter.Top.Value : 10;

      if (!aFilter.LastID.HasValue)
      {
        _res = _res.FindAll(x => IsFiltered5(x, aFilter))
          .OrderByDescending(x => x.ID)
          .Take(_top)
          .OrderBy(x => x.ID)
          .ToList();
      }
      else
        _res = _res
          .FindAll(x => IsFiltered5(x, aFilter))
          .Take(_top)
          .ToList();

      return _res;
    }
  }//end of class
}
