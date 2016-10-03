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
    void SendMessages(ConcurrentQueue<Event> aQueue);
  }

  public class MonikInstance
  {
    private IBaseSender FSender;
    private string FSourceName;
    private string FSourceInstance;

    private ConcurrentQueue<Event> FMsgQueue = new ConcurrentQueue<Event>();

    private Task FSenderTask;
    private readonly ManualResetEvent FNewMessageEvent = new ManualResetEvent(false);
    private readonly CancellationTokenSource FSenderCancellationTokenSource = new CancellationTokenSource();

    public int SendDelay = 1000; // in ms
    public uint AutoKeepAliveInterval = 10000; //in ms

    private CancellationTokenSource FAutoKeepAliveCancellationTokenSource;

    private Task FAutoKeepAliveTask;

    private bool FAutoKeepAlive;
    public bool AutoKeepAlive
    {
      get { return FAutoKeepAlive; }
      set
      {
        if (FAutoKeepAlive == value)
          return;

        FAutoKeepAlive = value;

        if (FAutoKeepAlive == false)
          FAutoKeepAliveCancellationTokenSource.Cancel();
        else
        {
          FAutoKeepAliveCancellationTokenSource = new CancellationTokenSource();
          FAutoKeepAliveTask = Task.Run(() => { OnAutoKeepAliveTask(); });
        }
      }
    }

    private void OnAutoKeepAliveTask()
    {
      while (!FAutoKeepAliveCancellationTokenSource.IsCancellationRequested)
      {
        Task.Delay((int)AutoKeepAliveInterval).Wait();

        KeepAlive();
      }
    }

    public MonikInstance(IBaseSender aSender, string aSourceName, string aSourceInstance)
    {
      FSender = aSender;
      FSourceName = aSourceName;
      FSourceInstance = aSourceInstance;
      FAutoKeepAlive = false;
      FAutoKeepAliveTask = null;
      FAutoKeepAliveCancellationTokenSource = null;

      FSenderTask = Task.Run(() => { OnSenderTask(); });
    }

    public void OnStop()
    {
      // TODO: is it correct?
      FNewMessageEvent.Set();
      FSenderCancellationTokenSource.Cancel();

      if (FAutoKeepAlive)
        FAutoKeepAliveCancellationTokenSource.Cancel();

      // TODO: may be mor efficient?
      Task.Delay(2000).Wait();
    }

    private void OnSenderTask()
    {
      while (!FSenderCancellationTokenSource.IsCancellationRequested)
      {
        FNewMessageEvent.WaitOne();
        Task.Delay(SendDelay).Wait();

        try
        {
          if (FMsgQueue.IsEmpty)
            continue;

          FSender.SendMessages(FMsgQueue);
        }
        catch { } // TODO: ???
        finally { FNewMessageEvent.Reset(); }
      }
    }

    private Event NewEvent()
    {
      return new Event()
      {
        Created = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds,
        Source = FSourceName, //Helper.Utf16ToUtf8(FSourceName),
        Instance = FSourceInstance//Helper.Utf16ToUtf8(FSourceInstance)
      };
    }

    private void PushLogToSend(string aBody, Log.Types.LevelType aLevel, Log.Types.SeverityType aSeverity, params object[] aParams)
    {
      string _text = string.Format(aBody, aParams);

      Event _msg = NewEvent();
      _msg.Lg = new Log()
      {
        Format = Log.Types.FormatType.Plain,
        Body = _text,//Helper.Utf16ToUtf8(_text),
        Level = aLevel,
        Severity = aSeverity
      };

      FMsgQueue.Enqueue(_msg);

      FNewMessageEvent.Set();
    }

    public void KeepAlive()
    {
      Event _msg = NewEvent();
      _msg.Ka = new Common.KeepAlive() { Interval = AutoKeepAliveInterval };

      FMsgQueue.Enqueue(_msg);

      FNewMessageEvent.Set();
    }

    public void SystemInfo(string aBody, params object[] aParams) { PushLogToSend(aBody, Log.Types.LevelType.System, Log.Types.SeverityType.Info, aParams); }
    public void SystemWarning(string aBody, params object[] aParams) { PushLogToSend(aBody, Log.Types.LevelType.System, Log.Types.SeverityType.Warning, aParams); }
    public void SystemError(string aBody, params object[] aParams) { PushLogToSend(aBody, Log.Types.LevelType.System, Log.Types.SeverityType.Error, aParams); }
    public void SystemFatal(string aBody, params object[] aParams) { PushLogToSend(aBody, Log.Types.LevelType.System, Log.Types.SeverityType.Fatal, aParams); }

    public void ApplicationInfo(string aBody, params object[] aParams) { PushLogToSend(aBody, Log.Types.LevelType.Application, Log.Types.SeverityType.Info, aParams); }
    public void ApplicationWarning(string aBody, params object[] aParams) { PushLogToSend(aBody, Log.Types.LevelType.Application, Log.Types.SeverityType.Warning, aParams); }
    public void ApplicationError(string aBody, params object[] aParams) { PushLogToSend(aBody, Log.Types.LevelType.Application, Log.Types.SeverityType.Error, aParams); }
    public void ApplicationFatal(string aBody, params object[] aParams) { PushLogToSend(aBody, Log.Types.LevelType.Application, Log.Types.SeverityType.Fatal, aParams); }

    public void LogicInfo(string aBody, params object[] aParams) { PushLogToSend(aBody, Log.Types.LevelType.Logic, Log.Types.SeverityType.Info, aParams); }
    public void LogicWarning(string aBody, params object[] aParams) { PushLogToSend(aBody, Log.Types.LevelType.Logic, Log.Types.SeverityType.Warning, aParams); }
    public void LogicError(string aBody, params object[] aParams) { PushLogToSend(aBody, Log.Types.LevelType.Logic, Log.Types.SeverityType.Error, aParams); }
    public void LogicFatal(string aBody, params object[] aParams) { PushLogToSend(aBody, Log.Types.LevelType.Logic, Log.Types.SeverityType.Fatal, aParams); }

    public void SecurityInfo(string aBody, params object[] aParams) { PushLogToSend(aBody, Log.Types.LevelType.Security, Log.Types.SeverityType.Info, aParams); }
    public void SecurityWarning(string aBody, params object[] aParams) { PushLogToSend(aBody, Log.Types.LevelType.Security, Log.Types.SeverityType.Warning, aParams); }
    public void SecurityError(string aBody, params object[] aParams) { PushLogToSend(aBody, Log.Types.LevelType.Security, Log.Types.SeverityType.Error, aParams); }
    public void SecurityFatal(string aBody, params object[] aParams) { PushLogToSend(aBody, Log.Types.LevelType.Security, Log.Types.SeverityType.Fatal, aParams); }
  }

  public class M
  {
    public static MonikInstance MainInstance { get; private set; } = null;

    public static MonikInstance I = null;
    public static MonikInstance I2 = null;

    public static void Initialize(IBaseSender aSender, string aSourceName, string aSourceInstance)
    {
      MainInstance = CreateAdditionalInstance(aSender, aSourceName, aSourceInstance);
    }

    public static MonikInstance CreateAdditionalInstance(IBaseSender aSender, string aSourceName, string aSourceInstance)
    {
      MonikInstance _res = new MonikInstance(aSender, aSourceName == null ? "" : aSourceName, aSourceInstance == null ? "" : aSourceInstance );
      return _res;
    }

    public static void OnStop() { MainInstance?.OnStop(); }

    public static void KeepAlive() { MainInstance?.KeepAlive(); }

    public static void SystemInfo(string aBody, params object[] aParams) { MainInstance?.SystemInfo(aBody, aParams); }
    public static void SystemWarning(string aBody, params object[] aParams) { MainInstance?.SystemWarning(aBody, aParams); }
    public static void SystemError(string aBody, params object[] aParams) { MainInstance?.SystemError(aBody, aParams); }
    public static void SystemFatal(string aBody, params object[] aParams) { MainInstance?.SystemFatal(aBody, aParams); }

    public static void ApplicationInfo(string aBody, params object[] aParams) { MainInstance?.ApplicationInfo(aBody, aParams); }
    public static void ApplicationWarning(string aBody, params object[] aParams) { MainInstance?.ApplicationWarning(aBody, aParams); }
    public static void ApplicationError(string aBody, params object[] aParams) { MainInstance?.ApplicationError(aBody, aParams); }
    public static void ApplicationFatal(string aBody, params object[] aParams) { MainInstance?.ApplicationFatal(aBody, aParams); }

    public static void LogicInfo(string aBody, params object[] aParams) { MainInstance?.LogicInfo(aBody, aParams); }
    public static void LogicWarning(string aBody, params object[] aParams) { MainInstance?.LogicWarning(aBody, aParams); }
    public static void LogicError(string aBody, params object[] aParams) { MainInstance?.LogicError(aBody, aParams); }
    public static void LogicFatal(string aBody, params object[] aParams) { MainInstance?.LogicFatal(aBody, aParams); }

    public static void SecurityInfo(string aBody, params object[] aParams) { MainInstance?.SecurityInfo(aBody, aParams); }
    public static void SecurityWarning(string aBody, params object[] aParams) { MainInstance?.SecurityWarning(aBody, aParams); }
    public static void SecurityError(string aBody, params object[] aParams) { MainInstance?.SecurityError(aBody, aParams); }
    public static void SecurityFatal(string aBody, params object[] aParams) { MainInstance?.SecurityFatal(aBody, aParams); }
  }

}
