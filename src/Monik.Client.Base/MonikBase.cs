using System;
using System.Collections.Concurrent;

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

            IntermediateMeasuresAccum = new ConcurrentDictionary<string, double>();
            IntermediateMeasuresGauge = new ConcurrentDictionary<string, double>();
        }

        public abstract void OnStop();

        protected Event NewEvent()
        {
            return new Event
            {
                Created = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Source = _sourceName,
                Instance = _instanceName
            };
        }

        protected abstract void OnNewMessage(Event msg);

        private void PrepareLogMessageAndRaise(string body, LevelType level, SeverityType severity, params object[] parameters)
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

            Event msg = NewEvent();
            msg.Lg = new Log()
            {
                Format = FormatType.Plain,
                Body = text,
                Level = level,
                Severity = severity
            };

            OnNewMessage(msg);
        }

        public void KeepAlive()
        {
            var msg = NewEvent();
            msg.Ka = new KeepAlive { Interval = _keepAliveInterval };

            OnNewMessage(msg);
        }

        public void SystemVerbose(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.System, SeverityType.Verbose, parameters);
        public void SystemInfo(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.System, SeverityType.Info, parameters);
        public void SystemWarning(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.System, SeverityType.Warning, parameters);
        public void SystemError(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.System, SeverityType.Error, parameters);
        public void SystemFatal(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.System, SeverityType.Fatal, parameters);
        public void ApplicationVerbose(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.Application, SeverityType.Verbose, parameters);
        public void ApplicationInfo(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.Application, SeverityType.Info, parameters);
        public void ApplicationWarning(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.Application, SeverityType.Warning, parameters);
        public void ApplicationError(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.Application, SeverityType.Error, parameters);
        public void ApplicationFatal(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.Application, SeverityType.Fatal, parameters);
        public void LogicVerbose(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.Logic, SeverityType.Verbose, parameters);
        public void LogicInfo(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.Logic, SeverityType.Info, parameters);
        public void LogicWarning(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.Logic, SeverityType.Warning, parameters);
        public void LogicError(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.Logic, SeverityType.Error, parameters);
        public void LogicFatal(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.Logic, SeverityType.Fatal, parameters);
        public void SecurityVerbose(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.Security, SeverityType.Verbose, parameters);
        public void SecurityInfo(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.Security, SeverityType.Info, parameters);
        public void SecurityWarning(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.Security, SeverityType.Warning, parameters);
        public void SecurityError(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.Security, SeverityType.Error, parameters);
        public void SecurityFatal(string body, params object[] parameters) => PrepareLogMessageAndRaise(body, LevelType.Security, SeverityType.Fatal, parameters);

        protected volatile ConcurrentDictionary<string, double> IntermediateMeasuresAccum;
        protected volatile ConcurrentDictionary<string, double> IntermediateMeasuresGauge;

        public void Measure(string metricName, AggregationType aggregate, double value)
        {
            if (aggregate == AggregationType.Accumulator)
                IntermediateMeasuresAccum.AddOrUpdate(metricName, value,
                    (key, existingVal) => existingVal + value);

            if (aggregate == AggregationType.Gauge)
                IntermediateMeasuresGauge.AddOrUpdate(metricName, value,
                    (key, existingVal) => (existingVal + value) / 2);

            OnNewMessage(null);
        }
    }//end of class
}
