using System;
using System.Text;

namespace Monik.Common
{
    public class MonikConsole : IMonik
    {
        protected virtual void LogToConsole(string body, LevelType level, SeverityType severity, params object[] parameters)
        {
            string text = "";

            try
            {
                text = parameters.Length == 0 ? body : string.Format(body, parameters);
            }
            catch
            {
                text = body;
            }

            Console.WriteLine($"{DateTime.Now.ToString("HH:mm")} {level.ToString()} {severity.ToString()} | {text}");
        }

        public void KeepAlive() { }

        public void OnStop() { }

        public void SystemVerbose(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.System, SeverityType.Verbose, aParams);
        }

        public void SystemInfo(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.System, SeverityType.Info, aParams);
        }

        public void SystemWarning(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.System, SeverityType.Warning, aParams);
        }

        public void SystemError(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.System, SeverityType.Error, aParams);
        }

        public void SystemFatal(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.System, SeverityType.Fatal, aParams);
        }

        public void ApplicationVerbose(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.Application, SeverityType.Verbose, aParams);
        }

        public void ApplicationInfo(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.Application, SeverityType.Info, aParams);
        }

        public void ApplicationWarning(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.Application, SeverityType.Warning, aParams);
        }

        public void ApplicationError(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.Application, SeverityType.Error, aParams);
        }

        public void ApplicationFatal(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.Application, SeverityType.Fatal, aParams);
        }

        public void LogicVerbose(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.Logic, SeverityType.Verbose, aParams);
        }

        public void LogicInfo(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.Logic, SeverityType.Info, aParams);
        }

        public void LogicWarning(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.Logic, SeverityType.Warning, aParams);
        }

        public void LogicError(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.Logic, SeverityType.Error, aParams);
        }

        public void LogicFatal(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.Logic, SeverityType.Fatal, aParams);
        }

        public void SecurityVerbose(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.Security, SeverityType.Verbose, aParams);
        }

        public void SecurityInfo(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.Security, SeverityType.Info, aParams);
        }

        public void SecurityWarning(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.Security, SeverityType.Warning, aParams);
        }

        public void SecurityError(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.Security, SeverityType.Error, aParams);
        }

        public void SecurityFatal(string aBody, params object[] aParams)
        {
            LogToConsole(aBody, LevelType.Security, SeverityType.Fatal, aParams);
        }
    }//end of class
}
