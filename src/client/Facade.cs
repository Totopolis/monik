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
  public class M
  {
    private static IClientControl FInstance = null;

    public static void Initialize(IClientSender aSender, string aSourceName, string aInstanceName, bool aAutoKeepAliveEnable = false)
    {
      IClientSettings _settings = new ClientSettings()
      {
        SourceName = aSourceName == null ? "" : aSourceName,
        InstanceName = aInstanceName == null ? "" : aInstanceName,
        AutoKeepAliveEnable = aAutoKeepAliveEnable
      };

      FInstance = new MonikInstance(aSender, _settings);
    }

    public static void OnStop() { FInstance?.OnStop(); }

    public static void KeepAlive() { FInstance?.KeepAlive(); }

    public static void SystemVerbose(string aBody, params object[] aParams) { FInstance?.SystemVerbose(aBody, aParams); }
    public static void SystemInfo(string aBody, params object[] aParams) { FInstance?.SystemInfo(aBody, aParams); }
    public static void SystemWarning(string aBody, params object[] aParams) { FInstance?.SystemWarning(aBody, aParams); }
    public static void SystemError(string aBody, params object[] aParams) { FInstance?.SystemError(aBody, aParams); }
    public static void SystemFatal(string aBody, params object[] aParams) { FInstance?.SystemFatal(aBody, aParams); }

    public static void ApplicationVerbose(string aBody, params object[] aParams) { FInstance?.ApplicationVerbose(aBody, aParams); }
    public static void ApplicationInfo(string aBody, params object[] aParams) { FInstance?.ApplicationInfo(aBody, aParams); }
    public static void ApplicationWarning(string aBody, params object[] aParams) { FInstance?.ApplicationWarning(aBody, aParams); }
    public static void ApplicationError(string aBody, params object[] aParams) { FInstance?.ApplicationError(aBody, aParams); }
    public static void ApplicationFatal(string aBody, params object[] aParams) { FInstance?.ApplicationFatal(aBody, aParams); }

    public static void LogicVerbose(string aBody, params object[] aParams) { FInstance?.LogicVerbose(aBody, aParams); }
    public static void LogicInfo(string aBody, params object[] aParams) { FInstance?.LogicInfo(aBody, aParams); }
    public static void LogicWarning(string aBody, params object[] aParams) { FInstance?.LogicWarning(aBody, aParams); }
    public static void LogicError(string aBody, params object[] aParams) { FInstance?.LogicError(aBody, aParams); }
    public static void LogicFatal(string aBody, params object[] aParams) { FInstance?.LogicFatal(aBody, aParams); }

    public static void SecurityVerbose(string aBody, params object[] aParams) { FInstance?.SecurityVerbose(aBody, aParams); }
    public static void SecurityInfo(string aBody, params object[] aParams) { FInstance?.SecurityInfo(aBody, aParams); }
    public static void SecurityWarning(string aBody, params object[] aParams) { FInstance?.SecurityWarning(aBody, aParams); }
    public static void SecurityError(string aBody, params object[] aParams) { FInstance?.SecurityError(aBody, aParams); }
    public static void SecurityFatal(string aBody, params object[] aParams) { FInstance?.SecurityFatal(aBody, aParams); }
  }
}
