using Gerakul.FastSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monik;
using Monik.Common;
using System.Diagnostics;
using Monik.Client;

namespace Monik.Service
{
  public class SourceInstanceCache : ISourceInstanceCache
  {
    private IRepository FRepository;

    private Dictionary<string, Source> FSources;
    private Dictionary<short, Source> FSourceMap;

    private Dictionary<int, Instance> FInstanceMap;
    private Dictionary<string, Instance> FSourceInstanceMap;

    public SourceInstanceCache(IRepository aRepository)
    {
      FRepository = aRepository;
      FSources = new Dictionary<string, Source>();
      FSourceMap = new Dictionary<short, Source>();
      FInstanceMap = new Dictionary<int, Instance>();
      FSourceInstanceMap = new Dictionary<string, Instance>();
      
      M.ApplicationInfo("SourceInstanceCache created");
    }

    public void OnStart()
    {
      var _sources = FRepository.GetAllSources();
      foreach (var _src in _sources)
        if (!FSources.ContainsKey(_src.Name))
        {
          FSourceMap.Add(_src.ID, _src);
          FSources.Add(_src.Name, _src);
        }
        else
          M.ApplicationError($"Database contains more than one same source name: {_src.Name}");

      var _instances = FRepository.GetAllInstances();
      foreach (var _ins in _instances)
        if (FSourceMap.ContainsKey(_ins.SourceID))
        {
          Source _src = FSourceMap[_ins.SourceID];
          string _key = $"{_src.Name}*{_ins.Name}";

          if (!FSourceInstanceMap.ContainsKey(_key))
          {
            _ins.SourceRef(_src);
            FInstanceMap.Add(_ins.ID, _ins);
            FSourceInstanceMap.Add(_key, _ins);
          }
          else
            M.ApplicationError($"Database contains more than one the same instance name '{_ins.Name}' for the source '{_src.Name}'");
        }
        else
          M.ApplicationError($"Database doesnt contains source(id={_ins.SourceID}) for the instance '{_ins.Name}'");
    }

    public void OnStop()
    {
    }

    public Source GetSourceByInstanceID(int aInstanceID)
    {
      return FInstanceMap.ContainsKey(aInstanceID) ? FInstanceMap[aInstanceID].SourceRef() : null;
    }

    public Instance GetInstanceByID(int aInstanceID)
    {
      return FInstanceMap.ContainsKey(aInstanceID) ? FInstanceMap[aInstanceID] : null;
    }

    public Instance CheckSourceAndInstance(string aSourceName, string aInstanceName)
    {
      string _key = $"{aSourceName}*{aInstanceName}";

      if (FSourceInstanceMap.ContainsKey(_key))
        return FSourceInstanceMap[_key];

      Source _src;

      if (!FSources.ContainsKey(aSourceName))
      {
        _src = new Source() { Name = aSourceName, Created = DateTime.UtcNow };
        FRepository.CreateNewSource(_src);

        FSources.Add(aSourceName, _src);
        FSourceMap.Add(_src.ID, _src);
      }
      else
        _src = FSources[aSourceName];

      Instance _ins = new Instance() { Name = aInstanceName, Created = DateTime.UtcNow, SourceID = _src.ID };
      FRepository.CreateNewInstance(_ins);

      _ins.SourceRef(_src);
      FInstanceMap.Add(_ins.ID, _ins);

      FSourceInstanceMap.Add(_key, _ins);

      return _ins;
    }
  }//end of class
}
