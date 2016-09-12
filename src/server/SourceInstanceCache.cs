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

  public class SourceInstanceCache
  {
    private string FConnectionString;
    private Dictionary<string, Source> FSources;
    private Dictionary<Source, InstanceMap> FInstances;

    public SourceInstanceCache(string aConnectionString)
    {
      FConnectionString = aConnectionString;
      FSources = new Dictionary<string, Source>();
      FInstances = new Dictionary<Source, InstanceMap>();
    }

    public void OnStart()
    {
      Dictionary<short, Source> _srcMap = new Dictionary<short, Source>();

      var _sources = SimpleCommand.ExecuteQuery<Source>(FConnectionString, "select * from [mon].[Source]");
      foreach (var _src in _sources)
      {
        _srcMap.Add(_src.ID, _src);
        FSources.Add(_src.Name, _src);
        FInstances.Add(_src, new InstanceMap());
      }

      var _instances = SimpleCommand.ExecuteQuery<Instance>(FConnectionString, "select * from [mon].[Instance]");
      foreach (var _ins in _instances)
      {
        Source _src = _srcMap[_ins.SourceID];
        FInstances[_src].Add(_ins.Name, _ins);
      }
    }

    public Tuple<short, int> CheckSourceAndInstance(string aSourceName, string aInstanceName)
    {
      Source _src;
      InstanceMap _map;
      Instance _ins;

      if (!FSources.ContainsKey(aSourceName))
      {
        _src = new Source() { Name = aSourceName, Created = DateTime.UtcNow };
        _src.ID = (short)MappedCommand.InsertAndGetId<Source>(FConnectionString, "[mon].[Source]", _src, "ID");

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
        _ins.ID = (int)MappedCommand.InsertAndGetId<Instance>(FConnectionString, "[mon].[Instance]", _ins, "ID");

        _map.Add(aInstanceName, _ins);
      }
      else
        _ins = _map[aInstanceName];

      return new Tuple<short, int>(_src.ID, _ins.ID);
    }
  }//end of class
}
