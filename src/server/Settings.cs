using Gerakul.FastSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monik;
using Monik.Common;
using Microsoft.Azure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Monik.Client;

namespace Monik.Service
{
  public class ServiceSettings : IServiceSettings
  {
    // TODO: use concurent when online updates
    private static Dictionary<string, string> FSettings = null;

    public void OnStart()
    {
      if (FSettings == null)
      {
        FSettings = new Dictionary<string, string>();

        var _proto = new { name = default(string), value = default(string) };
        var _settings = SimpleCommand.ExecuteQueryAnonymous(_proto, DBConnectionString, "select Name, Value from mon.Settings");
        foreach (var it in _settings)
          FSettings[it.name] = it.value;
      }
    }

    public void OnStop()
    {
    }

    public string CloudInstanceName { get { return RoleEnvironment.IsEmulated ? "Development" : "Production"; } }

    public int DayDeepKeepAlive { get { return int.Parse(FSettings["DayDeepKeepAlive"]); } }

    public int DayDeepLog { get { return int.Parse(FSettings["DayDeepLog"]); } }

    public string DBConnectionString { get { return CloudConfigurationManager.GetSetting("DBConnectionString"); } }

    public string OutcomingConnectionString { get { return FSettings["OutcomingConnectionString"]; } }

    public string OutcomingQueue { get { return FSettings["OutcomingQueue"]; } }

  }//end of class

  public class ServiceClientSettings : IClientSettings
  {
    private IServiceSettings FServiceSettings;

    public ServiceClientSettings(IServiceSettings aServiceSettings)
    {
      FServiceSettings = aServiceSettings;
    }

    public bool AutoKeepAliveEnable { get { return true; } set { throw new NotImplementedException(); } }

    public ushort AutoKeepAliveInterval { get { return 60; } set { throw new NotImplementedException(); } }

    public string InstanceName { get { return FServiceSettings.CloudInstanceName; } set { throw new NotImplementedException(); } }

    public ushort SendDelay { get { return 1; } set { throw new NotImplementedException(); } }

    public string SourceName { get { return "Monik"; } set { throw new NotImplementedException(); } }
  }
}
