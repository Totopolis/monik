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
  using InstanceMap = Dictionary<string, Instance>;

  public class Source
  {
    public short ID { get; set; }
    public DateTime Created { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
  }

  public class Instance
  {
    public int ID { get; set; }
    public DateTime Created { get; set; }
    public short SourceID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
  }

  public class SourceInstanceCache : ISourceInstanceCache
  {
    private IRepository FRepository;
    private Dictionary<string, Source> FSources;
    private Dictionary<Source, InstanceMap> FInstances;

    public SourceInstanceCache(IRepository aRepository)
    {
      FRepository = aRepository;
      FSources = new Dictionary<string, Source>();
      FInstances = new Dictionary<Source, InstanceMap>();
    }

    public void OnStart()
    {
      Dictionary<short, Source> _srcMap = new Dictionary<short, Source>();

      var _sources = FRepository.GetAllSources();
      foreach (var _src in _sources)
        if (!FSources.ContainsKey(_src.Name))
        {
          _srcMap.Add(_src.ID, _src);
          FSources.Add(_src.Name, _src);
          FInstances.Add(_src, new InstanceMap());
        }
        else
          M.ApplicationError("Database contains more than one same source name: " + _src.Name);

      var _instances = FRepository.GetAllInstances();
      foreach (var _ins in _instances)
        if (_srcMap.ContainsKey(_ins.SourceID))
        {
          Source _src = _srcMap[_ins.SourceID];

          if (!FInstances[_src].ContainsKey(_ins.Name))
            FInstances[_src].Add(_ins.Name, _ins);
          else
            M.ApplicationError("Database contains more than one the same instance name '{0}' for the source '{1}'", _ins.Name, _src.Name);
        }
        else
          M.ApplicationError("Database doesnot contains source(id={1}) for the instance '{0}'", _ins.Name, _ins.SourceID);
    }

    public void OnStop()
    {
    }

    public Instance CheckSourceAndInstance(string aSourceName, string aInstanceName)
    {
      Source _src;
      InstanceMap _map;
      Instance _ins;

      if (!FSources.ContainsKey(aSourceName))
      {
        _src = new Source() { Name = aSourceName, Created = DateTime.UtcNow };
        FRepository.CreateNewSource(_src);

        FSources.Add(aSourceName, _src);
        _map = new InstanceMap();
        FInstances.Add(_src, _map);
      }
      else
      {
        _src = FSources[aSourceName];
        _map = FInstances[_src];
      }

      if (!_map.ContainsKey(aInstanceName))
      {
        _ins = new Instance() { Name = aInstanceName, Created = DateTime.UtcNow, SourceID = _src.ID };
        FRepository.CreateNewInstance(_ins);

        _map.Add(aInstanceName, _ins);
      }
      else
        _ins = _map[aInstanceName];

      return _ins;
    }

    
  }//end of class
}
