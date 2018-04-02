using System;
using System.Text;

namespace Monik.Common
{
    public interface IMonik
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
