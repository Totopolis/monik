using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Monik.Client.Sender;
using Monik.Client.Settings;
using Monik.Common;

namespace Monik.Client.ClientControl
{
    public class MonikInstance : IClientControl
    {
        private readonly IClientSender _sender;

        private string FSourceName;
        private string FInstanceName;

        private ConcurrentQueue<Event> FMsgQueue = new ConcurrentQueue<Event>();

        private readonly ManualResetEvent        FNewMessageEvent               = new ManualResetEvent(false);
        private readonly CancellationTokenSource FSenderCancellationTokenSource = new CancellationTokenSource();

        private ushort FSendDelay;
        private ushort FAutoKeepAliveInterval;

        private CancellationTokenSource FAutoKeepAliveCancellationTokenSource;

        private bool FAutoKeepAliveEnable;

        private bool AutoKeepAliveEnable
        {
            get => FAutoKeepAliveEnable;
            set
            {
                if (FAutoKeepAliveEnable == value) return;

                FAutoKeepAliveEnable = value;

                if (FAutoKeepAliveEnable == false)
                    FAutoKeepAliveCancellationTokenSource.Cancel();
                else
                {
                    FAutoKeepAliveCancellationTokenSource = new CancellationTokenSource();

                    var cancellationToken = FAutoKeepAliveCancellationTokenSource.Token;
                    Task.Run(() => { OnAutoKeepAliveTask(cancellationToken); }, cancellationToken);
                }
            }
        }

        private void OnAutoKeepAliveTask(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                int msDelay = FAutoKeepAliveInterval * 1000;
                Task.Delay(msDelay, token).Wait(token);

                KeepAlive();
            }
        }

        public MonikInstance(IClientSender aSender, IClientSettings aSettings)
        {
            _sender = aSender;

            // TODO: when IDisposable from subscribe will be raise?

            FSourceName   = aSettings.SourceName;
            FInstanceName = aSettings.InstanceName;

            FSendDelay = aSettings.SendDelay;

            FAutoKeepAliveEnable                  = false;
            FAutoKeepAliveCancellationTokenSource = null;

            FAutoKeepAliveInterval = aSettings.AutoKeepAliveInterval;
            AutoKeepAliveEnable    = aSettings.AutoKeepAliveEnable;

            Task.Run(() => { OnSenderTask(); });
        }

        public void OnStop()
        {
            // TODO: is it correct?
            FNewMessageEvent.Set();
            FSenderCancellationTokenSource.Cancel();

            if (FAutoKeepAliveEnable) FAutoKeepAliveCancellationTokenSource.Cancel();

            // TODO: may be mor efficient?
            Task.Delay(2000).Wait();
        }

        private void OnSenderTask()
        {
            while (!FSenderCancellationTokenSource.IsCancellationRequested)
            {
                FNewMessageEvent.WaitOne();

                int msDelay = FSendDelay * 1000;
                Task.Delay(msDelay).Wait();

                try
                {
                    if (FMsgQueue.IsEmpty) continue;

                    _sender.SendMessages(FMsgQueue);
                }
                catch
                {
                    // TODO: ???  
                }
                finally
                {
                    FNewMessageEvent.Reset();
                }
            }
        }

        private Event NewEvent()
        {
            return new Event()
            {
                Created  = (long) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds,
                Source   = FSourceName,  //Helper.Utf16ToUtf8(FSourceName),
                Instance = FInstanceName //Helper.Utf16ToUtf8(FSourceInstance)
            };
        }

        private void PushLogToSend(string aBody, LevelType aLevel, SeverityType aSeverity, params object[] aParams)
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
                Format   = FormatType.Plain,
                Body     = text, //Helper.Utf16ToUtf8(_text),
                Level    = aLevel,
                Severity = aSeverity
            };

            FMsgQueue.Enqueue(msg);

            FNewMessageEvent.Set();
        }

        private void PushMetricToSend(string name, long value, MetricType metricType)
        {
            Event msg = NewEvent();

            msg.Metric = new Metric()
            {
                Name       = name,
                MetricType = metricType,
                Value      = value,
            };

            FMsgQueue.Enqueue(msg);

            FNewMessageEvent.Set();
        }

        public void KeepAlive()
        {
            Event msg = NewEvent();
            msg.Ka = new Common.KeepAlive() {Interval = FAutoKeepAliveInterval};

            FMsgQueue.Enqueue(msg);

            FNewMessageEvent.Set();
        }

        #region Logs

        [Obsolete("Use fun(string aBody)", true)] public void SystemVerbose     (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.System,      SeverityType.Verbose, aParams);
        [Obsolete("Use fun(string aBody)", true)] public void SystemInfo        (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.System,      SeverityType.Info,    aParams);
        [Obsolete("Use fun(string aBody)", true)] public void SystemWarning     (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.System,      SeverityType.Warning, aParams);
        [Obsolete("Use fun(string aBody)", true)] public void SystemError       (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.System,      SeverityType.Error,   aParams);
        [Obsolete("Use fun(string aBody)", true)] public void SystemFatal       (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.System,      SeverityType.Fatal,   aParams);
        [Obsolete("Use fun(string aBody)", true)] public void ApplicationVerbose(string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.Application, SeverityType.Verbose, aParams);
        [Obsolete("Use fun(string aBody)", true)] public void ApplicationInfo   (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.Application, SeverityType.Info,    aParams);
        [Obsolete("Use fun(string aBody)", true)] public void ApplicationWarning(string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.Application, SeverityType.Warning, aParams);
        [Obsolete("Use fun(string aBody)", true)] public void ApplicationError  (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.Application, SeverityType.Error,   aParams);
        [Obsolete("Use fun(string aBody)", true)] public void ApplicationFatal  (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.Application, SeverityType.Fatal,   aParams);
        [Obsolete("Use fun(string aBody)", true)] public void LogicVerbose      (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.Logic,       SeverityType.Verbose, aParams);
        [Obsolete("Use fun(string aBody)", true)] public void LogicInfo         (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.Logic,       SeverityType.Info,    aParams);
        [Obsolete("Use fun(string aBody)", true)] public void LogicWarning      (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.Logic,       SeverityType.Warning, aParams);
        [Obsolete("Use fun(string aBody)", true)] public void LogicError        (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.Logic,       SeverityType.Error,   aParams);
        [Obsolete("Use fun(string aBody)", true)] public void LogicFatal        (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.Logic,       SeverityType.Fatal,   aParams);
        [Obsolete("Use fun(string aBody)", true)] public void SecurityVerbose   (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.Security,    SeverityType.Verbose, aParams);
        [Obsolete("Use fun(string aBody)", true)] public void SecurityInfo      (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.Security,    SeverityType.Info,    aParams);
        [Obsolete("Use fun(string aBody)", true)] public void SecurityWarning   (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.Security,    SeverityType.Warning, aParams);
        [Obsolete("Use fun(string aBody)", true)] public void SecurityError     (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.Security,    SeverityType.Error,   aParams);
        [Obsolete("Use fun(string aBody)", true)] public void SecurityFatal     (string aBody, params object[] aParams) => PushLogToSend(aBody, LevelType.Security,    SeverityType.Fatal,   aParams);

        public void SystemVerbose     (string aBody) => PushLogToSend(aBody, LevelType.System,      SeverityType.Verbose);
        public void SystemInfo        (string aBody) => PushLogToSend(aBody, LevelType.System,      SeverityType.Info);
        public void SystemWarning     (string aBody) => PushLogToSend(aBody, LevelType.System,      SeverityType.Warning);
        public void SystemError       (string aBody) => PushLogToSend(aBody, LevelType.System,      SeverityType.Error);
        public void SystemFatal       (string aBody) => PushLogToSend(aBody, LevelType.System,      SeverityType.Fatal);
        public void ApplicationVerbose(string aBody) => PushLogToSend(aBody, LevelType.Application, SeverityType.Verbose);
        public void ApplicationInfo   (string aBody) => PushLogToSend(aBody, LevelType.Application, SeverityType.Info);
        public void ApplicationWarning(string aBody) => PushLogToSend(aBody, LevelType.Application, SeverityType.Warning);
        public void ApplicationError  (string aBody) => PushLogToSend(aBody, LevelType.Application, SeverityType.Error);
        public void ApplicationFatal  (string aBody) => PushLogToSend(aBody, LevelType.Application, SeverityType.Fatal);
        public void LogicVerbose      (string aBody) => PushLogToSend(aBody, LevelType.Logic,       SeverityType.Verbose);
        public void LogicInfo         (string aBody) => PushLogToSend(aBody, LevelType.Logic,       SeverityType.Info);
        public void LogicWarning      (string aBody) => PushLogToSend(aBody, LevelType.Logic,       SeverityType.Warning);
        public void LogicError        (string aBody) => PushLogToSend(aBody, LevelType.Logic,       SeverityType.Error);
        public void LogicFatal        (string aBody) => PushLogToSend(aBody, LevelType.Logic,       SeverityType.Fatal);
        public void SecurityVerbose   (string aBody) => PushLogToSend(aBody, LevelType.Security,    SeverityType.Verbose);
        public void SecurityInfo      (string aBody) => PushLogToSend(aBody, LevelType.Security,    SeverityType.Info);
        public void SecurityWarning   (string aBody) => PushLogToSend(aBody, LevelType.Security,    SeverityType.Warning);
        public void SecurityError     (string aBody) => PushLogToSend(aBody, LevelType.Security,    SeverityType.Error);
        public void SecurityFatal     (string aBody) => PushLogToSend(aBody, LevelType.Security,    SeverityType.Fatal);

        #endregion

        #region Metrics

        public void Metric(string name, long value, MetricType metricType) => PushMetricToSend(name, value, metricType);

        #endregion
    }
}