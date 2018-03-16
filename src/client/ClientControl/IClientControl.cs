using System;
using Monik.Common;

namespace Monik.Client.ClientControl
{
    public interface IClientControl
	{
        #region obsolete best before May 2018
        [Obsolete("Use func(string aBody)", true)]void SystemVerbose(string aBody, params object[] aParams);
		[Obsolete("Use func(string aBody)", true)]void SystemInfo   (string aBody, params object[] aParams);
		[Obsolete("Use func(string aBody)", true)]void SystemWarning(string aBody, params object[] aParams);
		[Obsolete("Use func(string aBody)", true)]void SystemError  (string aBody, params object[] aParams);
		[Obsolete("Use func(string aBody)", true)]void SystemFatal  (string aBody, params object[] aParams);

		[Obsolete("Use func(string aBody)", true)]void ApplicationVerbose(string aBody, params object[] aParams);
		[Obsolete("Use func(string aBody)", true)]void ApplicationInfo   (string aBody, params object[] aParams);
		[Obsolete("Use func(string aBody)", true)]void ApplicationWarning(string aBody, params object[] aParams);
		[Obsolete("Use func(string aBody)", true)]void ApplicationError  (string aBody, params object[] aParams);
		[Obsolete("Use func(string aBody)", true)]void ApplicationFatal  (string aBody, params object[] aParams);
        
		[Obsolete("Use func(string aBody)", true)]void LogicVerbose(string aBody, params object[] aParams);
		[Obsolete("Use func(string aBody)", true)]void LogicInfo   (string aBody, params object[] aParams);
		[Obsolete("Use func(string aBody)", true)]void LogicWarning(string aBody, params object[] aParams);
		[Obsolete("Use func(string aBody)", true)]void LogicError  (string aBody, params object[] aParams);
		[Obsolete("Use func(string aBody)", true)]void LogicFatal  (string aBody, params object[] aParams);

		[Obsolete("Use func(string aBody)", true)]void SecurityVerbose(string aBody, params object[] aParams);
		[Obsolete("Use func(string aBody)", true)]void SecurityInfo   (string aBody, params object[] aParams);
		[Obsolete("Use func(string aBody)", true)]void SecurityWarning(string aBody, params object[] aParams);
		[Obsolete("Use func(string aBody)", true)]void SecurityError  (string aBody, params object[] aParams);
        [Obsolete("Use func(string aBody)", true)]void SecurityFatal  (string aBody, params object[] aParams);
        
	    #endregion

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