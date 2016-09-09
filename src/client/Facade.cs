using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.ServiceBus.Messaging;
using System.IO;
using System.Threading;
using Monik.Common;

namespace Monik.Client
{
  public interface IBaseSender
  {
    void SendLogs(ConcurrentQueue<Log> aQueue);
  }

  public class MonikInstance
  {
    private IBaseSender FSender;
    private string FSourceName;
    private string FSourceInstance;

    private static ConcurrentQueue<Log> FLogQueue = new ConcurrentQueue<Log>();

    private static Task FSenderTask;
    private static readonly ManualResetEvent FNewMessageEvent = new ManualResetEvent(false);
    private static readonly CancellationTokenSource FCancellationTokenSource = new CancellationTokenSource();

    public MonikInstance(IBaseSender aSender, string aSourceName, string aSourceInstance)
    {
      FSender = aSender;
      FSourceName = aSourceName;
      FSourceInstance = aSourceInstance;

      FSenderTask = Task.Run(() => { OnSenderTask(); });
    }

    public void Stop()
    {
      // TODO: is it correct?
      FNewMessageEvent.Set();
      FCancellationTokenSource.Cancel();

      // TODO: may be mor efficient?
      Task.Delay(2000);
    }

    private void OnSenderTask()
    {
      while (!FCancellationTokenSource.IsCancellationRequested)
      {
        FNewMessageEvent.WaitOne();
        Task.Delay(1000); // TODO: use param, by stopping not use delay

        try
        {
          if (FLogQueue.IsEmpty)
            continue;

          FSender.SendLogs(FLogQueue);
        }
        catch { } // TODO: ???
        finally { FNewMessageEvent.Reset(); }
      }
    }

    private Log NewLog()
    {
      return new Log()
      {
        Created = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds,
        Level = Log.Types.LevelType.Application,
        Severity = Log.Types.SeverityType.Info,
        Source = Helper.Utf16ToUtf8(FSourceName),
        Instance = Helper.Utf16ToUtf8(FSourceInstance),
        Format = Log.Types.FormatType.Plain
      };
    }

    private void PushMessageToSend(string aBody, Log.Types.LevelType aLevel, Log.Types.SeverityType aSeverity)
    {
      Log _msg = NewLog();
      _msg.Body = Helper.Utf16ToUtf8(aBody);
      _msg.Level = aLevel;
      _msg.Severity = aSeverity;

      FLogQueue.Enqueue(_msg);

      FNewMessageEvent.Set();
    }

    public void KeepAlive()
    {
      throw new NotImplementedException();
    }

    public void SystemInfo(string aBody) { PushMessageToSend(aBody, Log.Types.LevelType.System, Log.Types.SeverityType.Info); }
    public void SystemWarning(string aBody) { PushMessageToSend(aBody, Log.Types.LevelType.System, Log.Types.SeverityType.Warning); }
    public void SystemError(string aBody) { PushMessageToSend(aBody, Log.Types.LevelType.System, Log.Types.SeverityType.Error); }
    public void SystemFatal(string aBody) { PushMessageToSend(aBody, Log.Types.LevelType.System, Log.Types.SeverityType.Fatal); }

    public void ApplicationInfo(string aBody) { PushMessageToSend(aBody, Log.Types.LevelType.Application, Log.Types.SeverityType.Info); }
    public void ApplicationWarning(string aBody) { PushMessageToSend(aBody, Log.Types.LevelType.Application, Log.Types.SeverityType.Warning); }
    public void ApplicationError(string aBody) { PushMessageToSend(aBody, Log.Types.LevelType.Application, Log.Types.SeverityType.Error); }
    public void ApplicationFatal(string aBody) { PushMessageToSend(aBody, Log.Types.LevelType.Application, Log.Types.SeverityType.Fatal); }

    public void LogicInfo(string aBody) { PushMessageToSend(aBody, Log.Types.LevelType.Logic, Log.Types.SeverityType.Info); }
    public void LogicWarning(string aBody) { PushMessageToSend(aBody, Log.Types.LevelType.Logic, Log.Types.SeverityType.Warning); }
    public void LogicError(string aBody) { PushMessageToSend(aBody, Log.Types.LevelType.Logic, Log.Types.SeverityType.Error); }
    public void LogicFatal(string aBody) { PushMessageToSend(aBody, Log.Types.LevelType.Logic, Log.Types.SeverityType.Fatal); }

    public void SecurityInfo(string aBody) { PushMessageToSend(aBody, Log.Types.LevelType.Security, Log.Types.SeverityType.Info); }
    public void SecurityWarning(string aBody) { PushMessageToSend(aBody, Log.Types.LevelType.Security, Log.Types.SeverityType.Warning); }
    public void SecurityError(string aBody) { PushMessageToSend(aBody, Log.Types.LevelType.Security, Log.Types.SeverityType.Error); }
    public void SecurityFatal(string aBody) { PushMessageToSend(aBody, Log.Types.LevelType.Security, Log.Types.SeverityType.Fatal); }
  }

  public class M
  {
    private static MonikInstance MainInstance = null;

    public static MonikInstance I = null;
    public static MonikInstance I2 = null;

    public static void Initialize(IBaseSender aSender, string aSourceName, string aSourceInstance)
    {
      MainInstance = CreateAdditionalInstance(aSender, aSourceName, aSourceInstance);
    }

    public static MonikInstance CreateAdditionalInstance(IBaseSender aSender, string aSourceName, string aSourceInstance)
    {
      MonikInstance _res = new MonikInstance(aSender, aSourceName, aSourceInstance);
      return _res;
    }

    public static void Stop() { MainInstance?.Stop(); }

    public static void KeepAlive() { MainInstance?.KeepAlive(); }

    public static void SystemInfo(string aBody) { MainInstance?.SystemInfo(aBody); }
    public static void SystemWarning(string aBody) { MainInstance?.SystemWarning(aBody); }
    public static void SystemError(string aBody) { MainInstance?.SystemError(aBody); }
    public static void SystemFatal(string aBody) { MainInstance?.SystemFatal(aBody); }

    public static void ApplicationInfo(string aBody) { MainInstance?.ApplicationInfo(aBody); }
    public static void ApplicationWarning(string aBody) { MainInstance?.ApplicationWarning(aBody); }
    public static void ApplicationError(string aBody) { MainInstance?.ApplicationError(aBody); }
    public static void ApplicationFatal(string aBody) { MainInstance?.ApplicationFatal(aBody); }

    public static void LogicInfo(string aBody) { MainInstance?.LogicInfo(aBody); }
    public static void LogicWarning(string aBody) { MainInstance?.LogicWarning(aBody); }
    public static void LogicError(string aBody) { MainInstance?.LogicError(aBody); }
    public static void LogicFatal(string aBody) { MainInstance?.LogicFatal(aBody); }

    public static void SecurityInfo(string aBody) { MainInstance?.SecurityInfo(aBody); }
    public static void SecurityWarning(string aBody) { MainInstance?.SecurityWarning(aBody); }
    public static void SecurityError(string aBody) { MainInstance?.SecurityError(aBody); }
    public static void SecurityFatal(string aBody) { MainInstance?.SecurityFatal(aBody); }
  }

}
