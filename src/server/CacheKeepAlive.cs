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
  public class CacheKeepAlive : ICacheKeepAlive
  {
    private IRepository FRepository;
    private ISourceInstanceCache FCache;
    private IClientControl FControl;

    private Dictionary<int, KeepAlive_> FStatus;

    public CacheKeepAlive(IRepository aRepository, ISourceInstanceCache aCache, IClientControl aControl)
    {
      FRepository = aRepository;
      FCache = aCache;
      FControl = aControl;

      FStatus = new Dictionary<int, KeepAlive_>();

      FControl.ApplicationVerbose("CacheKeepAlive created");
    }

    public void OnStart()
    {
      // fill from database
      LastKeepAliveID = FRepository.GetMaxKeepAliveID();

      List<KeepAlive_> _top = FRepository.GetLastKeepAlive(1000);
      _top.Reverse();

      // fill current status
      foreach (var ka in _top)
        if (!FStatus.ContainsKey(ka.InstanceID))
          FStatus.Add(ka.InstanceID, ka);
        else
          if (FStatus[ka.InstanceID].Created < ka.Created)
          FStatus[ka.InstanceID] = ka;

      FControl.ApplicationVerbose("CacheKeepAlive started");
    }

    public void OnStop()
    {
      // nothing
    }

    public long LastKeepAliveID { get; private set; }

    public void OnNewKeepAlive(KeepAlive_ aKeepAlive)
    {
      lock (this)
      {
        LastKeepAliveID = aKeepAlive.ID;

        if (FStatus.ContainsKey(aKeepAlive.InstanceID))
        {
          if (FStatus[aKeepAlive.InstanceID].Created < aKeepAlive.Created)
            FStatus[aKeepAlive.InstanceID] = aKeepAlive;
        }
        else
          FStatus.Add(aKeepAlive.InstanceID, aKeepAlive);
      } // TODO: optimize lock
    }

    public List<KeepAlive_> GetKeepAlive(LogsFilter[] aFilters)
    {
      lock (this)
      {
        List<KeepAlive_> _res = FStatus.Values.ToList();
        _res.RemoveAll(ka => !FCache.IsDefaultInstance(ka.InstanceID));

        return _res;
      } // TODO: optimize lock
    }

    // TODO: use filter
    public List<KeepAlive_> GetKeepAlive2(KeepAliveRequest aFilter)
    {
      lock (this)
      {
        List<KeepAlive_> _res = FStatus.Values.ToList();

        if (aFilter.Groups.Length == 0 && aFilter.Instances.Length == 0)
        {
          _res.RemoveAll(ka => !FCache.IsDefaultInstance(ka.InstanceID));
          return _res;
        }
        else
        {
          var _filteredRes = new List<KeepAlive_>();

          foreach (var ka in _res)
          {
            foreach (var gr in aFilter.Groups)
              if (FCache.IsInstanceInGroup(ka.InstanceID, gr))
                _filteredRes.Add(ka);

            foreach (var inst in aFilter.Instances)
              if (inst == ka.InstanceID)
                _filteredRes.Add(ka);
          }

          return _filteredRes;
        }
      } // TODO: optimize lock
    }

  }//end of class
}
