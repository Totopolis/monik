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

	        DayDeepKeepAlive          = int.Parse(_settings[nameof(DayDeepKeepAlive)]);
	        DayDeepLog                = int.Parse(_settings[nameof(DayDeepLog)]);
	        OutcomingConnectionString = _settings[nameof(OutcomingConnectionString)];
	        OutcomingQueue            = _settings[nameof(OutcomingQueue)];
	        MetricDeepMinutes         = int.Parse(_settings[nameof(MetricDeepMinutes)]);
	        MetricInterval            = int.Parse(_settings[nameof(MetricInterval)]);
        }

	    public virtual string InstanceName       { get; } = "UnknownInstance";
	    public virtual string DbConnectionString { get; } = "Your connection string";

	    public int    DayDeepKeepAlive          { get; }
	    public int    MetricDeepMinutes         { get; }
	    public int    MetricInterval            { get; }
	    public int    DayDeepLog                { get; } 
	    public string OutcomingConnectionString { get; }
	    public string OutcomingQueue            { get; }
	} //end of class
}
