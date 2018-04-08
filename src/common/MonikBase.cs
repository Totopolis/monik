using System;
using System.Collections.Concurrent;
using System.Text;

namespace Monik.Common
{
    public abstract class MonikBase : IMonik
    {
        protected readonly string _sourceName;
        protected readonly string _instanceName;
        protected readonly ushort _keepAliveInterval;

        public MonikBase(string sourceName, string instanceName, ushort keepAliveInterval)
        {
            _sourceName = sourceName;
            _instanceName = instanceName;
            _keepAliveInterval = keepAliveInterval;
        }

        public abstract void OnStop();

        protected Event NewEvent()
        {
            return new Event()
            {
                Created = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds,
                Source = _sourceName, //Helper.Utf16ToUtf8(FSourceName),
                Instance = _instanceName //Helper.Utf16ToUtf8(FSourceInstance)
            };
        }

        protected abstract void OnNewMessage(Event msg);

        private void PrepareLogMessageAndRaise(string aBody, LevelType aLevel, SeverityType aSeverity, params object[] aParams)
        {
            string text = "";

            try
            {
                text = aParams.Length == 0 ? aBody : string.Format(aBody, aParams);
            }
            catch
            {
                text = aBody;
            }

            Event msg = NewEvent();
            msg.Lg = new Log()
            {
                Format = FormatType.Plain,
                Body = text, //Helper.Utf16ToUtf8(_text),
                Level = aLevel,
                Severity = aSeverity
            };

            OnNewMessage(msg);
        }

        public void KeepAlive()
        {
            Event msg = NewEvent();
            msg.Ka = new Common.KeepAlive() { Interval = _keepAliveInterval };

            OnNewMessage(msg);
        }

        public void SystemVerbose(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.System, SeverityType.Verbose, aParams);
        }

        public void SystemInfo(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.System, SeverityType.Info, aParams);
        }

        public void SystemWarning(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.System, SeverityType.Warning, aParams);
        }

        public void SystemError(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.System, SeverityType.Error, aParams);
        }

        public void SystemFatal(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.System, SeverityType.Fatal, aParams);
        }

        public void ApplicationVerbose(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.Application, SeverityType.Verbose, aParams);
        }

        public void ApplicationInfo(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.Application, SeverityType.Info, aParams);
        }

        public void ApplicationWarning(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.Application, SeverityType.Warning, aParams);
        }

        public void ApplicationError(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.Application, SeverityType.Error, aParams);
        }

        public void ApplicationFatal(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.Application, SeverityType.Fatal, aParams);
        }

        public void LogicVerbose(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.Logic, SeverityType.Verbose, aParams);
        }

        public void LogicInfo(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.Logic, SeverityType.Info, aParams);
        }

        public void LogicWarning(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.Logic, SeverityType.Warning, aParams);
        }

        public void LogicError(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.Logic, SeverityType.Error, aParams);
        }

        public void LogicFatal(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.Logic, SeverityType.Fatal, aParams);
        }

        public void SecurityVerbose(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.Security, SeverityType.Verbose, aParams);
        }

        public void SecurityInfo(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.Security, SeverityType.Info, aParams);
        }

        public void SecurityWarning(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.Security, SeverityType.Warning, aParams);
        }

        public void SecurityError(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.Security, SeverityType.Error, aParams);
        }

        public void SecurityFatal(string aBody, params object[] aParams)
        {
            PrepareLogMessageAndRaise(aBody, LevelType.Security, SeverityType.Fatal, aParams);
        }
    }//end of class
}
