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
  public class MonikInstance : IMonikClient
  {
    private IBaseSender FSender;
    private string FSourceName;
    private string FSourceInstance;

    private ConcurrentQueue<Event> FMsgQueue = new ConcurrentQueue<Event>();

    private Task FSenderTask;
    private readonly ManualResetEvent FNewMessageEvent = new ManualResetEvent(false);
    private readonly CancellationTokenSource FSenderCancellationTokenSource = new CancellationTokenSource();

    public ushort SendDelay { get; set; } = 1; // in sec, min 1
    public ushort AutoKeepAliveInterval { get; set; } = 60; //in sec, min 1

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
        int _msDelay = AutoKeepAliveInterval * 1000;
        Task.Delay(_msDelay).Wait();

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

        int _msDelay = SendDelay * 1000;
        Task.Delay(_msDelay).Wait();

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
      string _text = "";

      try
      {
        _text = aParams.Length == 0 ? aBody : string.Format(aBody, aParams);
      }
      catch
      {
        _text = aBody;
      }

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
}
