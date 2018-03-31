using Monik.Common;

namespace Monik.Client
{
    public class M
    {
        private static IMonik _instance = null;

        public static void Initialize(IClientSender aSender, string aSourceName, string aInstanceName,
            bool aAutoKeepAliveEnable = false)
        {
            IMonikSettings settings = new ClientSettings()
            {
                SourceName = aSourceName ?? "",
                InstanceName = aInstanceName ?? "",
                AutoKeepAliveEnable = aAutoKeepAliveEnable
            };

            _instance = new MonikInstance(aSender, settings);
        }

        public static void OnStop()
        {
            _instance?.OnStop();
        }

        public static void KeepAlive()
        {
            _instance?.KeepAlive();
        }

        public static void SystemVerbose(string aBody, params object[] aParams)
        {
            _instance?.SystemVerbose(aBody, aParams);
        }

        public static void SystemInfo(string aBody, params object[] aParams)
        {
            _instance?.SystemInfo(aBody, aParams);
        }

        public static void SystemWarning(string aBody, params object[] aParams)
        {
            _instance?.SystemWarning(aBody, aParams);
        }

        public static void SystemError(string aBody, params object[] aParams)
        {
            _instance?.SystemError(aBody, aParams);
        }

        public static void SystemFatal(string aBody, params object[] aParams)
        {
            _instance?.SystemFatal(aBody, aParams);
        }

        public static void ApplicationVerbose(string aBody, params object[] aParams)
        {
            _instance?.ApplicationVerbose(aBody, aParams);
        }

        public static void ApplicationInfo(string aBody, params object[] aParams)
        {
            _instance?.ApplicationInfo(aBody, aParams);
        }

        public static void ApplicationWarning(string aBody, params object[] aParams)
        {
            _instance?.ApplicationWarning(aBody, aParams);
        }

        public static void ApplicationError(string aBody, params object[] aParams)
        {
            _instance?.ApplicationError(aBody, aParams);
        }

        public static void ApplicationFatal(string aBody, params object[] aParams)
        {
            _instance?.ApplicationFatal(aBody, aParams);
        }

        public static void LogicVerbose(string aBody, params object[] aParams)
        {
            _instance?.LogicVerbose(aBody, aParams);
        }

        public static void LogicInfo(string aBody, params object[] aParams)
        {
            _instance?.LogicInfo(aBody, aParams);
        }

        public static void LogicWarning(string aBody, params object[] aParams)
        {
            _instance?.LogicWarning(aBody, aParams);
        }

        public static void LogicError(string aBody, params object[] aParams)
        {
            _instance?.LogicError(aBody, aParams);
        }

        public static void LogicFatal(string aBody, params object[] aParams)
        {
            _instance?.LogicFatal(aBody, aParams);
        }

        public static void SecurityVerbose(string aBody, params object[] aParams)
        {
            _instance?.SecurityVerbose(aBody, aParams);
        }

        public static void SecurityInfo(string aBody, params object[] aParams)
        {
            _instance?.SecurityInfo(aBody, aParams);
        }

        public static void SecurityWarning(string aBody, params object[] aParams)
        {
            _instance?.SecurityWarning(aBody, aParams);
        }

        public static void SecurityError(string aBody, params object[] aParams)
        {
            _instance?.SecurityError(aBody, aParams);
        }

        public static void SecurityFatal(string aBody, params object[] aParams)
        {
            _instance?.SecurityFatal(aBody, aParams);
        }
    }
}
