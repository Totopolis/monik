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
  public interface IClientSender
  {
    void SendMessages(ConcurrentQueue<Event> aQueue);
  }

  public interface IClientSettings
  {/*
    /// <summary>
    /// Name of the source (app name, service or smthing else)
    /// </summary>
    IObservable<string> SourceNameProperty { get; }
    /// <summary>
    /// Name of the concrete instance (production, development, instace number, etc...)
    /// </summary>
    IObservable<string> InstanceNameProperty { get; }
    /// <summary>
    /// How many seconds to wait after execute monik method, but before message will be send
    /// </summary>
    IObservable<ushort> SendDelayProperty { get; }
    /// <summary>
    /// If AutoKeepAlive enabled, KeepAlive() will be raised per each interval (in seconds)
    /// </summary>
    IObservable<ushort> AutoKeepAliveIntervalProperty { get; }
    /// <summary>
    /// Enable/disable automatic KeepAlive() method executor
    /// </summary>
    IObservable<bool> AutoKeepAliveEnableProperty { get; }*/

    string SourceName { get; set; }
    string InstanceName { get; set; }
    ushort SendDelay { get; set; }
    ushort AutoKeepAliveInterval { get; set; }
    bool AutoKeepAliveEnable { get; set; }

    // TODO: logging level cut support
  }

  public interface IClientControl
  {
    void SystemVerbose(string aBody, params object[] aParams);
    void SystemInfo(string aBody, params object[] aParams);
    void SystemWarning(string aBody, params object[] aParams);
    void SystemError(string aBody, params object[] aParams);
    void SystemFatal(string aBody, params object[] aParams);

    void ApplicationVerbose(string aBody, params object[] aParams);
    void ApplicationInfo(string aBody, params object[] aParams);
    void ApplicationWarning(string aBody, params object[] aParams);
    void ApplicationError(string aBody, params object[] aParams);
    void ApplicationFatal(string aBody, params object[] aParams);

    void LogicVerbose(string aBody, params object[] aParams);
    void LogicInfo(string aBody, params object[] aParams);
    void LogicWarning(string aBody, params object[] aParams);
    void LogicError(string aBody, params object[] aParams);
    void LogicFatal(string aBody, params object[] aParams);

    void SecurityVerbose(string aBody, params object[] aParams);
    void SecurityInfo(string aBody, params object[] aParams);
    void SecurityWarning(string aBody, params object[] aParams);
    void SecurityError(string aBody, params object[] aParams);
    void SecurityFatal(string aBody, params object[] aParams);

    void KeepAlive();

    void OnStop();
  }
}