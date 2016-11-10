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
    private IClientControl FControl;

    private Dictionary<int, KeepAlive_> FStatus;

    public CacheKeepAlive(IRepository aRepository, IClientControl aControl)
    {
      FRepository = aRepository;
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
      LastKeepAliveID = aKeepAlive.ID;

      if (FStatus.ContainsKey(aKeepAlive.InstanceID))
      {
        if (FStatus[aKeepAlive.InstanceID].Created < aKeepAlive.Created)
          FStatus[aKeepAlive.InstanceID] = aKeepAlive;
      }
      else
        FStatus.Add(aKeepAlive.InstanceID, aKeepAlive);
    }

    public List<KeepAlive_> GetKeepAlive(LogsFilter[] aFilters)
    {
      //List<KeepAlive_> _res = new List<KeepAlive_>();

      if (aFilters != null || aFilters.Length > 0)
      {
      }

      return FStatus.Values.ToList();
    }

  }//end of class
}
