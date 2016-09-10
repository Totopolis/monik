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

    public void Stop()
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
        Source = Helper.Utf16ToUtf8(FSourceName),
        Instance = Helper.Utf16ToUtf8(FSourceInstance)
      };
    }

    private void PushLogToSend(string aBody, Log.Types.LevelType aLevel, Log.Types.SeverityType aSeverity)
    {
      Event _msg = NewEvent();
      _msg.Lg = new Log()
      {
        Format = Log.Types.FormatType.Plain,
        Body = Helper.Utf16ToUtf8(aBody),
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

    public void SystemInfo(string aBody) { PushLogToSend(aBody, Log.Types.LevelType.System, Log.Types.SeverityType.Info); }
    public void SystemWarning(string aBody) { PushLogToSend(aBody, Log.Types.LevelType.System, Log.Types.SeverityType.Warning); }
    public void SystemError(string aBody) { PushLogToSend(aBody, Log.Types.LevelType.System, Log.Types.SeverityType.Error); }
    public void SystemFatal(string aBody) { PushLogToSend(aBody, Log.Types.LevelType.System, Log.Types.SeverityType.Fatal); }

    public void ApplicationInfo(string aBody) { PushLogToSend(aBody, Log.Types.LevelType.Application, Log.Types.SeverityType.Info); }
    public void ApplicationWarning(string aBody) { PushLogToSend(aBody, Log.Types.LevelType.Application, Log.Types.SeverityType.Warning); }
    public void ApplicationError(string aBody) { PushLogToSend(aBody, Log.Types.LevelType.Application, Log.Types.SeverityType.Error); }
    public void ApplicationFatal(string aBody) { PushLogToSend(aBody, Log.Types.LevelType.Application, Log.Types.SeverityType.Fatal); }

    public void LogicInfo(string aBody) { PushLogToSend(aBody, Log.Types.LevelType.Logic, Log.Types.SeverityType.Info); }
    public void LogicWarning(string aBody) { PushLogToSend(aBody, Log.Types.LevelType.Logic, Log.Types.SeverityType.Warning); }
    public void LogicError(string aBody) { PushLogToSend(aBody, Log.Types.LevelType.Logic, Log.Types.SeverityType.Error); }
    public void LogicFatal(string aBody) { PushLogToSend(aBody, Log.Types.LevelType.Logic, Log.Types.SeverityType.Fatal); }

    public void SecurityInfo(string aBody) { PushLogToSend(aBody, Log.Types.LevelType.Security, Log.Types.SeverityType.Info); }
    public void SecurityWarning(string aBody) { PushLogToSend(aBody, Log.Types.LevelType.Security, Log.Types.SeverityType.Warning); }
    public void SecurityError(string aBody) { PushLogToSend(aBody, Log.Types.LevelType.Security, Log.Types.SeverityType.Error); }
    public void SecurityFatal(string aBody) { PushLogToSend(aBody, Log.Types.LevelType.Security, Log.Types.SeverityType.Fatal); }
  }

  public class M
  {
    public static MonikInstance MainInstance { get; private set; }

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
