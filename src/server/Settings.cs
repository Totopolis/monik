using System.Collections.Generic;
using Gerakul.FastSql;

namespace Monik.Service
{
	public class ServiceSettings : IServiceSettings
	{
		// TODO: use concurent when online updates
		private static Dictionary<string, string> _settings = null;

	    public ServiceSettings()
	    {
	        if (_settings == null)
	        {
	            _settings = new Dictionary<string, string>();

	            var proto = new {name = default(string), value = default(string)};
	            var settings = SimpleCommand.ExecuteQueryAnonymous(proto, DbConnectionString,
	                "select Name, Value from mon.Settings");
	            foreach (var it in settings)
	                _settings[it.name] = it.value;
	        }

	        DayDeepKeepAlive          = int.Parse(_settings["DayDeepKeepAlive"]);
	        DayDeepLog                = int.Parse(_settings["DayDeepLog"]);
	        OutcomingConnectionString = _settings["OutcomingConnectionString"];
	        OutcomingQueue            = _settings["OutcomingQueue"];
	    }

	    public virtual string InstanceName { get; } = "UnknownInstance";

	    public int DayDeepKeepAlive { get; }

	    public int DayDeepLog { get; } 

	    public virtual string DbConnectionString { get; } = "Your connection string";

	    public string OutcomingConnectionString { get; }

	    public string OutcomingQueue { get; }
	} //end of class
}
