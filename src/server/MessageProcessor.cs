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

  public class MessageProcessor
  {
    private string FConnectionString;

    public MessageProcessor(string aConnectionString)
    {
      FConnectionString = aConnectionString;
    }

    private long FLastLogID;
    private long FLastKeepAliveID;

    public void OnStart()
    {
      var _res1 = SimpleCommand.ExecuteScalar(FConnectionString, "select max(ID) from [mon].[Log]");
      FLastLogID = _res1 == System.DBNull.Value ? 0 : (long)_res1;

      var _res2 = SimpleCommand.ExecuteScalar(FConnectionString, "select max(ID) from [mon].[KeepAlive]");
      FLastKeepAliveID = _res2 == System.DBNull.Value ? 0 : (long)_res2;

      Task.Run(() =>
      {
        while (true)
        {
          var src = DateTime.UtcNow;
          var hm = new DateTime(src.Year, src.Month, src.Day, src.Hour, 0, 0);

          // TODO: fix for more correct stats, may be multiply inserts by exceptions
          try
          {
            // insert new hour
            var stat = new { Hour = hm, LastLogID = FLastLogID, LastKeepAliveID = FLastKeepAliveID };
            MappedCommand.Insert(FConnectionString, "[mon].[HourStat]", stat);

            // cleanup logs
            var _logDeep = int.Parse(Settings.GetValue("DayDeepLog"));
            var _logThreshold = SimpleCommand.ExecuteScalar(FConnectionString, "select max(LastLogID) from mon.HourStat where Hour < DATEADD(DAY, -@p0, GETDATE())", _logDeep);
            if (_logThreshold != System.DBNull.Value)
            {
              long _val = (long)_logThreshold;
              SimpleCommand.ExecuteNonQuery(FConnectionString, "delete from mon.Log where ID < @p0", _val);
            }

            // cleanup keep-alive
            var _kaDeep = int.Parse(Settings.GetValue("DayDeepKeepAlive"));
            var _kaThreshold = SimpleCommand.ExecuteScalar(FConnectionString, "select max(LastKeepAliveID) from mon.HourStat where Hour < DATEADD(DAY, -@p0, GETDATE())", _kaDeep);
            if (_kaThreshold != System.DBNull.Value)
            {
              long _val = (long)_kaThreshold;
              SimpleCommand.ExecuteNonQuery(FConnectionString, "delete from mon.KeepAlive where ID < @p0", _val);
            }
          }
          catch(Exception _e)
          {
            M.ApplicationError("MessageProcessor.HourStatsThread: " + _e.Message);

            // delayed retry logic with magic number
            Task.Delay(1000).Wait();
            continue;
          }

          var hm2 = new DateTime(src.Year, src.Month, src.Day, src.Hour, 0, 0).AddHours(1);
          var delay = hm2 - DateTime.UtcNow;

          Task.Delay(delay).Wait();
        }
      });
    }

    public void OnStop()
    {
      // TODO: stop HourStat thread
    }

    public void Process(Event aEvent, Tuple<short, int> aSourceAndInstance)
    {
      switch (aEvent.MsgCase)
      {
        case Event.MsgOneofCase.None:
          throw new NotSupportedException("Bad event type");
        case Event.MsgOneofCase.Ka:
          var _ka = WriteKeepAlive(aEvent, aSourceAndInstance);
          FLastKeepAliveID = _ka.ID;
          break;
        case Event.MsgOneofCase.Lg:
          var _lg = WriteLog(aEvent, aSourceAndInstance);
          FLastLogID = _lg.ID;
          break;
        default:
          throw new NotSupportedException("Bad event type");
      }
    }

    private KeepAlive_ WriteKeepAlive(Event aEventLog, Tuple<short, int> aSourceAndInstance)
    {
      KeepAlive_ _row = new KeepAlive_()
      {
        Created = Helper.FromMillisecondsSinceUnixEpoch(aEventLog.Created),
        Received = DateTime.UtcNow,
        InstanceID = aSourceAndInstance.Item2
      };

      _row.ID = (int)MappedCommand.InsertAndGetId<KeepAlive_>(FConnectionString, "[mon].[KeepAlive]", _row, "ID");

      return _row;
    }

    private Log_ WriteLog(Event aEventLog, Tuple<short, int> aSourceAndInstance)
    {
      Log_ _row = new Log_()
      {
        Created = Helper.FromMillisecondsSinceUnixEpoch(aEventLog.Created),
        Received = DateTime.UtcNow,
        Level = (byte)aEventLog.Lg.Level,
        Severity = (byte)aEventLog.Lg.Severity,
        InstanceID = aSourceAndInstance.Item2,
        Format = (byte)aEventLog.Lg.Format,
        Body = aEventLog.Lg.Body,
        Tags = aEventLog.Lg.Tags
      };

      _row.ID = (int)MappedCommand.InsertAndGetId<Log_>(FConnectionString, "[mon].[Log]", _row, "ID");

      return _row;
    }
  }
}
