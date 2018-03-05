using System.Collections.Generic;
using Gerakul.FastSql;

namespace Monik.Service
{
	public class ServiceSettings : IServiceSettings
	{
		// TODO: use concurent when online updates
		private static Dictionary<string, string> _settings = null;

		public void OnStart()
		{
			if (_settings == null)
			{
				_settings = new Dictionary<string, string>();

				var proto = new {name = default(string), value = default(string)};
				var settings = SimpleCommand.ExecuteQueryAnonymous(proto, DbConnectionString, "select Name, Value from mon.Settings");
				foreach (var it in settings)
					_settings[it.name] = it.value;
			}
		}

		public void OnStop()
		{
		}

		public virtual string InstanceName
		{
		    get { return "UnknownInstance"; }
		}

		public int DayDeepKeepAlive
		{
			get { return int.Parse(_settings["DayDeepKeepAlive"]); }
		}

		public int DayDeepLog
		{
			get { return int.Parse(_settings["DayDeepLog"]); }
		}

		public virtual string DbConnectionString
		{
		    get { return "Your connection string"; }
		}

		public string OutcomingConnectionString
		{
			get { return _settings["OutcomingConnectionString"]; }
		}

		public string OutcomingQueue
		{
			get { return _settings["OutcomingQueue"]; }
		}

	} //end of class
}
