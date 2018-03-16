using System.Configuration;
using MonikService.Core.Settings;

namespace MonikWinService.Settings
{
    public class WinServiceSettings : ServiceSettings
    {
        public override string InstanceName       { get; } = "Dev";
        public override string DbConnectionString { get; } = ConfigurationManager.AppSettings["DBConnectionString"];
    }
}