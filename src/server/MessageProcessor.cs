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
  public class Log_
  {
    public long ID { get; set; }
    public DateTime Created { get; set; }
    public DateTime Received { get; set; }
    public byte Level { get; set; }
    public byte Severity { get; set; }
    public short SourceID { get; set; }
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
    public short SourceID { get; set; }
    public int InstanceID { get; set; }
  }

  public class MessageProcessor
  {
    private string FConnectionString;

    public MessageProcessor(string aConnectionString)
    {
      FConnectionString = aConnectionString;
    }

    public void Process(Event aEvent, Tuple<short, int> aSourceAndInstance)
    {
      switch (aEvent.MsgCase)
      {
        case Event.MsgOneofCase.None:
          throw new NotSupportedException("Bad event type");
        case Event.MsgOneofCase.Ka:
          WriteKeepAlive(aEvent, aSourceAndInstance);
          break;
        case Event.MsgOneofCase.Lg:
          WriteLog(aEvent, aSourceAndInstance);
          break;
        default:
          throw new NotSupportedException("Bad event type");
      }
    }

    private void WriteKeepAlive(Event aEventLog, Tuple<short, int> aSourceAndInstance)
    {
      KeepAlive_ _row = new KeepAlive_()
      {
        Created = Helper.FromMillisecondsSinceUnixEpoch(aEventLog.Created),
        Received = DateTime.UtcNow,
        SourceID = aSourceAndInstance.Item1,
        InstanceID = aSourceAndInstance.Item2
      };

      _row.ID = (int)MappedCommand.InsertAndGetId<KeepAlive_>(FConnectionString, "[mon].[KeepAlive]", _row, "ID");
    }

    private void WriteLog(Event aEventLog, Tuple<short, int> aSourceAndInstance)
    {
      Log_ _row = new Log_()
      {
        Created = Helper.FromMillisecondsSinceUnixEpoch(aEventLog.Created),
        Received = DateTime.UtcNow,
        Level = (byte)aEventLog.Lg.Level,
        Severity = (byte)aEventLog.Lg.Severity,
        SourceID = aSourceAndInstance.Item1,
        InstanceID = aSourceAndInstance.Item2,
        Format = (byte)aEventLog.Lg.Format,
        Body = aEventLog.Lg.Body,
        Tags = aEventLog.Lg.Tags
      };

      _row.ID = (int)MappedCommand.InsertAndGetId<Log_>(FConnectionString, "[mon].[Log]", _row, "ID");
    }
  }
}
