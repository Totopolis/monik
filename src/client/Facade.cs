using Monik.Common;

namespace Monik.Client
{
    public class M
    {
        private static IMonik _instance = null;

        public static void Initialize(IMonikSender sender, string sourceName, string instanceName,
            bool autoKeepAliveEnable = false)
        {
            IMonikSettings settings = new ClientSettings()
            {
                SourceName = sourceName ?? "",
                InstanceName = instanceName ?? "",
                AutoKeepAliveEnable = autoKeepAliveEnable
            };

            _instance = new MonikClient(sender, settings);
        }

        public static void OnStop()
        {
            _instance?.OnStop();
        }

        public static void Measure(string metricName, AggregationType aggregate, double value)
        {
            _instance?.Measure(metricName, aggregate, value);
        }

        public static void KeepAlive()
        {
            _instance?.KeepAlive();
        }

        public static void SystemVerbose(string body, params object[] parameters)
        {
            _instance?.SystemVerbose(body, parameters);
        }

        public static void SystemInfo(string body, params object[] parameters)
        {
            _instance?.SystemInfo(body, parameters);
        }

        public static void SystemWarning(string body, params object[] parameters)
        {
            _instance?.SystemWarning(body, parameters);
        }

        public static void SystemError(string body, params object[] parameters)
        {
            _instance?.SystemError(body, parameters);
        }

        public static void SystemFatal(string body, params object[] parameters)
        {
            _instance?.SystemFatal(body, parameters);
        }

        public static void ApplicationVerbose(string body, params object[] parameters)
        {
            _instance?.ApplicationVerbose(body, parameters);
        }

        public static void ApplicationInfo(string body, params object[] parameters)
        {
            _instance?.ApplicationInfo(body, parameters);
        }

        public static void ApplicationWarning(string body, params object[] parameters)
        {
            _instance?.ApplicationWarning(body, parameters);
        }

        public static void ApplicationError(string body, params object[] parameters)
        {
            _instance?.ApplicationError(body, parameters);
        }

        public static void ApplicationFatal(string body, params object[] parameters)
        {
            _instance?.ApplicationFatal(body, parameters);
        }

        public static void LogicVerbose(string body, params object[] parameters)
        {
            _instance?.LogicVerbose(body, parameters);
        }

        public static void LogicInfo(string body, params object[] parameters)
        {
            _instance?.LogicInfo(body, parameters);
        }

        public static void LogicWarning(string body, params object[] parameters)
        {
            _instance?.LogicWarning(body, parameters);
        }

        public static void LogicError(string body, params object[] parameters)
        {
            _instance?.LogicError(body, parameters);
        }

        public static void LogicFatal(string body, params object[] parameters)
        {
            _instance?.LogicFatal(body, parameters);
        }

        public static void SecurityVerbose(string body, params object[] parameters)
        {
            _instance?.SecurityVerbose(body, parameters);
        }

        public static void SecurityInfo(string body, params object[] parameters)
        {
            _instance?.SecurityInfo(body, parameters);
        }

        public static void SecurityWarning(string body, params object[] parameters)
        {
            _instance?.SecurityWarning(body, parameters);
        }

        public static void SecurityError(string body, params object[] parameters)
        {
            _instance?.SecurityError(body, parameters);
        }

        public static void SecurityFatal(string body, params object[] parameters)
        {
            _instance?.SecurityFatal(body, parameters);
        }
    }
}
