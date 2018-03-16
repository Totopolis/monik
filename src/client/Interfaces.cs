using System;
using System.Collections.Concurrent;
using Monik.Common;

namespace Monik.Client
{
	public interface IClientSender
	{
		void SendMessages(ConcurrentQueue<Event> aQueue);
	}

	public interface IClientSettings
	{
/*
    /// <summary>
    /// Name of the source (app name, service or smthing else)
    /// </summary>
    IObservable<string> SourceNameProperty { get; }
    /// <summary>
    /// Name of the concrete instance (production, development, instace number, etc...)
    /// </summary>
    IObservable<string> InstanceNameProperty { get; }
    /// <summary>
    /// How many seconds to wait after execute monik method, but before message will be send
    /// </summary>
    IObservable<ushort> SendDelayProperty { get; }
    /// <summary>
    /// If AutoKeepAlive enabled, KeepAlive() will be raised per each interval (in seconds)
    /// </summary>
    IObservable<ushort> AutoKeepAliveIntervalProperty { get; }
    /// <summary>
    /// Enable/disable automatic KeepAlive() method executor
    /// </summary>
    IObservable<bool> AutoKeepAliveEnableProperty { get; }*/

		string SourceName { get; set; }
		string InstanceName { get; set; }
		ushort SendDelay { get; set; }
		ushort AutoKeepAliveInterval { get; set; }
		bool AutoKeepAliveEnable { get; set; }

		// TODO: logging level cut support
	}

	public interface IClientControl
	{
		[Obsolete("Use fun(string aBody)", true)]void SystemVerbose(string aBody, params object[] aParams);
		[Obsolete("Use fun(string aBody)", true)]void SystemInfo   (string aBody, params object[] aParams);
		[Obsolete("Use fun(string aBody)", true)]void SystemWarning(string aBody, params object[] aParams);
		[Obsolete("Use fun(string aBody)", true)]void SystemError  (string aBody, params object[] aParams);
		[Obsolete("Use fun(string aBody)", true)]void SystemFatal  (string aBody, params object[] aParams);

		[Obsolete("Use fun(string aBody)", true)]void ApplicationVerbose(string aBody, params object[] aParams);
		[Obsolete("Use fun(string aBody)", true)]void ApplicationInfo   (string aBody, params object[] aParams);
		[Obsolete("Use fun(string aBody)", true)]void ApplicationWarning(string aBody, params object[] aParams);
		[Obsolete("Use fun(string aBody)", true)]void ApplicationError  (string aBody, params object[] aParams);
		[Obsolete("Use fun(string aBody)", true)]void ApplicationFatal  (string aBody, params object[] aParams);

		[Obsolete("Use fun(string aBody)", true)]void LogicVerbose(string aBody, params object[] aParams);
		[Obsolete("Use fun(string aBody)", true)]void LogicInfo   (string aBody, params object[] aParams);
		[Obsolete("Use fun(string aBody)", true)]void LogicWarning(string aBody, params object[] aParams);
		[Obsolete("Use fun(string aBody)", true)]void LogicError  (string aBody, params object[] aParams);
		[Obsolete("Use fun(string aBody)", true)]void LogicFatal  (string aBody, params object[] aParams);

		[Obsolete("Use fun(string aBody)", true)]void SecurityVerbose(string aBody, params object[] aParams);
		[Obsolete("Use fun(string aBody)", true)]void SecurityInfo   (string aBody, params object[] aParams);
		[Obsolete("Use fun(string aBody)", true)]void SecurityWarning(string aBody, params object[] aParams);
		[Obsolete("Use fun(string aBody)", true)]void SecurityError  (string aBody, params object[] aParams);
        [Obsolete("Use fun(string aBody)", true)]void SecurityFatal  (string aBody, params object[] aParams);

		void SystemVerbose(string aBody);
		void SystemInfo   (string aBody);
		void SystemWarning(string aBody);
		void SystemError  (string aBody);
		void SystemFatal  (string aBody);

		void ApplicationVerbose(string aBody);
		void ApplicationInfo   (string aBody);
		void ApplicationWarning(string aBody);
		void ApplicationError  (string aBody);
		void ApplicationFatal  (string aBody);

		void LogicVerbose(string aBody);
		void LogicInfo   (string aBody);
		void LogicWarning(string aBody);
		void LogicError  (string aBody);
		void LogicFatal  (string aBody);

		void SecurityVerbose(string aBody);
		void SecurityInfo   (string aBody);
		void SecurityWarning(string aBody);
		void SecurityError  (string aBody);
		void SecurityFatal  (string aBody);

		void KeepAlive();

	    void Metric(string name, long value, MetricType metricType);
            
        void OnStop();
	}
}