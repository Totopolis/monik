using System;
using System.Text;

namespace Monik.Common
{
    public interface IMonik
    {
        void SystemVerbose(string body, params object[] parameters);
        void SystemInfo(string body, params object[] parameters);
        void SystemWarning(string body, params object[] parameters);
        void SystemError(string body, params object[] parameters);
        void SystemFatal(string body, params object[] parameters);

        void ApplicationVerbose(string body, params object[] parameters);
        void ApplicationInfo(string body, params object[] parameters);
        void ApplicationWarning(string body, params object[] parameters);
        void ApplicationError(string body, params object[] parameters);
        void ApplicationFatal(string body, params object[] parameters);

        void LogicVerbose(string body, params object[] parameters);
        void LogicInfo(string body, params object[] parameters);
        void LogicWarning(string body, params object[] parameters);
        void LogicError(string body, params object[] parameters);
        void LogicFatal(string body, params object[] parameters);

        void SecurityVerbose(string body, params object[] parameters);
        void SecurityInfo(string body, params object[] parameters);
        void SecurityWarning(string body, params object[] parameters);
        void SecurityError(string body, params object[] parameters);
        void SecurityFatal(string body, params object[] parameters);

        void KeepAlive();

        void Measure(string metricName, AggregationType aggregate, double value);

        void OnStop();
    }
}
