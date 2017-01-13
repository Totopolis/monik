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

    private Source FSourceRef = null;
    public Source SourceRef() { return FSourceRef; }
    public void SourceRef(Source aSrc) { FSourceRef = aSrc; }
  }

  public class Group
  {
    public short ID { get; set; }
    public string Name { get; set; }
    public bool IsDefault { get; set; }
    public string Description { get; set; }

    public List<int> Instances { get; set; } = new List<int>();
  }

  public class Log_
  {
    public long ID { get; set; }
    public DateTime Created { get; set; }
    public DateTime Received { get; set; }
    public byte Level { get; set; }
    public byte Severity { get; set; }
    public int InstanceID { get; set; }
    public byte Format { get; set; }
    public string Body { get; set; }
    public string Tags { get; set; }
  }

  public class KeepAlive_
  {
    public long ID { get; set; }
    public DateTime Created { get; set; }
    public DateTime Received { get; set; }
    public int InstanceID { get; set; }
  }

  public enum Order
  {
    Asc,
    Desc
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

    public bool EnableCutoffSeverity { get; set; }
    public int CutoffSeverity { get; set; }
  }

  public class LogRequest
  {
    public short[] Groups { get; set; } = new short[0];
    public int[] Instances { get; set; } = new int[0];

    public long? LastID { get; set; }
    public byte? SeverityCutoff { get; set; }
    public byte? Level { get; set; }
    public int? Top { get; set; }
  }

  public class KeepAliveRequest
  {
    public short[] Groups { get; set; } = new short[0];
    public int[] Instances { get; set; } = new int[0];
  }
}
