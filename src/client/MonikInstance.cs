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
  public class MonikInstance : IClientControl
  {
    private IClientSender FSender;
    private IClientSettings FSettings;

    private string FSourceName;
    private string FInstanceName;

    private ConcurrentQueue<Event> FMsgQueue = new ConcurrentQueue<Event>();

    private Task FSenderTask;
    private readonly ManualResetEvent FNewMessageEvent = new ManualResetEvent(false);
    private readonly CancellationTokenSource FSenderCancellationTokenSource = new CancellationTokenSource();

    private ushort FSendDelay;
    private ushort FAutoKeepAliveInterval;

    private CancellationTokenSource FAutoKeepAliveCancellationTokenSource;

    private Task FAutoKeepAliveTask;

    private bool FAutoKeepAliveEnable;
    private bool AutoKeepAliveEnable
    {
      get { return FAutoKeepAliveEnable; }
      set
      {
        if (FAutoKeepAliveEnable == value)
          return;

        FAutoKeepAliveEnable = value;

        if (FAutoKeepAliveEnable == false)
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
        int _msDelay = FAutoKeepAliveInterval * 1000;
        Task.Delay(_msDelay).Wait();

        KeepAlive();
      }
    }

    public MonikInstance(IClientSender aSender, IClientSettings aSettings)
    {
      FSender = aSender;
      FSettings = aSettings;

      // TODO: when IDisposable from subscribe will be raise?

      FSourceName = FSettings.SourceName;
      FInstanceName = FSettings.InstanceName;

      FSendDelay = FSettings.SendDelay;

      FAutoKeepAliveEnable = false;
      FAutoKeepAliveTask = null;
      FAutoKeepAliveCancellationTokenSource = null;

      FAutoKeepAliveInterval = FSettings.AutoKeepAliveInterval;
      AutoKeepAliveEnable = FSettings.AutoKeepAliveEnable;

      FSenderTask = Task.Run(() => { OnSenderTask(); });
    }

    public void OnStop()
    {
      // TODO: is it correct?
      FNewMessageEvent.Set();
      FSenderCancellationTokenSource.Cancel();

      if (FAutoKeepAliveEnable)
        FAutoKeepAliveCancellationTokenSource.Cancel();

      // TODO: may be mor efficient?
      Task.Delay(2000).Wait();
    }

    private void OnSenderTask()
    {
      while (!FSenderCancellationTokenSource.IsCancellationRequested)
      {
        FNewMessageEvent.WaitOne();

        int _msDelay = FSendDelay * 1000;
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
        Instance = FInstanceName//Helper.Utf16ToUtf8(FSourceInstance)
      };
    }

    private void PushLogToSend(string aBody, LevelType aLevel, SeverityType aSeverity, params object[] aParams)
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
        Format = FormatType.Plain,
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
      _msg.Ka = new Common.KeepAlive() { Interval = FAutoKeepAliveInterval };

      FMsgQueue.Enqueue(_msg);

      FNewMessageEvent.Set();
    }

    public void SystemVerbose(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.System, SeverityType.Verbose, aParams); }
    public void SystemInfo(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.System, SeverityType.Info, aParams); }
    public void SystemWarning(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.System, SeverityType.Warning, aParams); }
    public void SystemError(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.System, SeverityType.Error, aParams); }
    public void SystemFatal(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.System, SeverityType.Fatal, aParams); }

    public void ApplicationVerbose(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.Application, SeverityType.Verbose, aParams); }
    public void ApplicationInfo(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.Application, SeverityType.Info, aParams); }
    public void ApplicationWarning(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.Application, SeverityType.Warning, aParams); }
    public void ApplicationError(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.Application, SeverityType.Error, aParams); }
    public void ApplicationFatal(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.Application, SeverityType.Fatal, aParams); }

    public void LogicVerbose(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.Logic, SeverityType.Verbose, aParams); }
    public void LogicInfo(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.Logic, SeverityType.Info, aParams); }
    public void LogicWarning(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.Logic, SeverityType.Warning, aParams); }
    public void LogicError(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.Logic, SeverityType.Error, aParams); }
    public void LogicFatal(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.Logic, SeverityType.Fatal, aParams); }

    public void SecurityVerbose(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.Security, SeverityType.Verbose, aParams); }
    public void SecurityInfo(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.Security, SeverityType.Info, aParams); }
    public void SecurityWarning(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.Security, SeverityType.Warning, aParams); }
    public void SecurityError(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.Security, SeverityType.Error, aParams); }
    public void SecurityFatal(string aBody, params object[] aParams) { PushLogToSend(aBody, LevelType.Security, SeverityType.Fatal, aParams); }
  }
}
