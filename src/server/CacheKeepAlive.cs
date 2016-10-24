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

    public CacheKeepAlive(IRepository aRepository)
    {
      FRepository = aRepository;
      M.ApplicationInfo("CacheKeepAlive created");
    }

    public void OnStart()
    {
      // load from database

      // 1. last IDs
      LastKeepAliveID = FRepository.GetMaxKeepAliveID();

      // 3. load top ka
    }

    public void OnStop()
    {
      // nothing
    }

    public long LastKeepAliveID { get; private set; }

    public void OnNewKeepAlive(KeepAlive_ aKeepAlive)
    {
      LastKeepAliveID = aKeepAlive.ID;
    }

  }//end of class
}
