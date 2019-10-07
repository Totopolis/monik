using System;

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

        public void SystemVerbose(string body, params object[] parameters) => LogToConsole(body, LevelType.System, SeverityType.Verbose, parameters);
        public void SystemInfo(string body, params object[] parameters) => LogToConsole(body, LevelType.System, SeverityType.Info, parameters);
        public void SystemWarning(string body, params object[] parameters) => LogToConsole(body, LevelType.System, SeverityType.Warning, parameters);
        public void SystemError(string body, params object[] parameters) => LogToConsole(body, LevelType.System, SeverityType.Error, parameters);
        public void SystemFatal(string body, params object[] parameters) => LogToConsole(body, LevelType.System, SeverityType.Fatal, parameters);
        public void ApplicationVerbose(string body, params object[] parameters) => LogToConsole(body, LevelType.Application, SeverityType.Verbose, parameters);
        public void ApplicationInfo(string body, params object[] parameters) => LogToConsole(body, LevelType.Application, SeverityType.Info, parameters);
        public void ApplicationWarning(string body, params object[] parameters) => LogToConsole(body, LevelType.Application, SeverityType.Warning, parameters);
        public void ApplicationError(string body, params object[] parameters) => LogToConsole(body, LevelType.Application, SeverityType.Error, parameters);
        public void ApplicationFatal(string body, params object[] parameters) => LogToConsole(body, LevelType.Application, SeverityType.Fatal, parameters);
        public void LogicVerbose(string body, params object[] parameters) => LogToConsole(body, LevelType.Logic, SeverityType.Verbose, parameters);
        public void LogicInfo(string body, params object[] parameters) => LogToConsole(body, LevelType.Logic, SeverityType.Info, parameters);
        public void LogicWarning(string body, params object[] parameters) => LogToConsole(body, LevelType.Logic, SeverityType.Warning, parameters);
        public void LogicError(string body, params object[] parameters) => LogToConsole(body, LevelType.Logic, SeverityType.Error, parameters);
        public void LogicFatal(string body, params object[] parameters) => LogToConsole(body, LevelType.Logic, SeverityType.Fatal, parameters);
        public void SecurityVerbose(string body, params object[] parameters) => LogToConsole(body, LevelType.Security, SeverityType.Verbose, parameters);
        public void SecurityInfo(string body, params object[] parameters) => LogToConsole(body, LevelType.Security, SeverityType.Info, parameters);
        public void SecurityWarning(string body, params object[] parameters) => LogToConsole(body, LevelType.Security, SeverityType.Warning, parameters);
        public void SecurityError(string body, params object[] parameters) => LogToConsole(body, LevelType.Security, SeverityType.Error, parameters);
        public void SecurityFatal(string body, params object[] parameters) => LogToConsole(body, LevelType.Security, SeverityType.Fatal, parameters);

        public void Measure(string metricName, AggregationType aggregate, double value) => throw new NotImplementedException();

    }//end of class
}
