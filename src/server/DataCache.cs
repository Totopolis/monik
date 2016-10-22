using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monik;
using Monik.Common;

namespace Monik.Service
{
  public class DataCache : IDataCache
  {
    private IRepository FRepository;

    public DataCache(IRepository aRepository)
    {
      FRepository = aRepository;
    }

    void IObject.OnStart()
    {
      // load from database
    }

    void IObject.OnStop()
    {
      // nothing
    }

    void NewLog() { }
    void NewKeepAlive() { }

  }//end of class
}
