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
    public static IMonikClient MainInstance { get; private set; } = null;

    public static IMonikClient I = null;
    public static IMonikClient I2 = null;

    public static void Initialize(IBaseSender aSender, string aSourceName, string aSourceInstance)
    {
      MainInstance = CreateAdditionalInstance(aSender, aSourceName, aSourceInstance);
    }

    public static IMonikClient CreateAdditionalInstance(IBaseSender aSender, string aSourceName, string aSourceInstance)
    {
      IMonikClient _res = new MonikInstance(aSender, aSourceName == null ? "" : aSourceName, aSourceInstance == null ? "" : aSourceInstance );
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
