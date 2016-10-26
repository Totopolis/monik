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

  public interface IMonikClient
  {
    /// <summary>
    /// How many seconds to wait after execute monik method, but before message will be send
    /// </summary>
    ushort SendDelay { get; set; }
    /// <summary>
    /// If AutoKeepAlive enabled, KeepAlive() will be raised per each interval (in seconds)
    /// </summary>
    ushort AutoKeepAliveInterval { get; set; }
    /// <summary>
    /// Enable/disable automatic KeepAlive() method executor
    /// </summary>
    bool AutoKeepAlive { get; set; }

    void SystemInfo(string aBody, params object[] aParams);
    void SystemWarning(string aBody, params object[] aParams);
    void SystemError(string aBody, params object[] aParams);
    void SystemFatal(string aBody, params object[] aParams);

    void ApplicationInfo(string aBody, params object[] aParams);
    void ApplicationWarning(string aBody, params object[] aParams);
    void ApplicationError(string aBody, params object[] aParams);
    void ApplicationFatal(string aBody, params object[] aParams);

    void LogicInfo(string aBody, params object[] aParams);
    void LogicWarning(string aBody, params object[] aParams);
    void LogicError(string aBody, params object[] aParams);
    void LogicFatal(string aBody, params object[] aParams);

    void SecurityInfo(string aBody, params object[] aParams);
    void SecurityWarning(string aBody, params object[] aParams);
    void SecurityError(string aBody, params object[] aParams);
    void SecurityFatal(string aBody, params object[] aParams);

    void KeepAlive();

    void OnStop();
  }
}