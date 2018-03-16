using Microsoft.Azure;
using Microsoft.WindowsAzure.ServiceRuntime;
using MonikService.Core.Settings;

namespace MonikWorker.Settings
{
    public class CloudServiceSettings : ServiceSettings
    {
        public override string InstanceName       { get; } = RoleEnvironment.IsEmulated ? "Dev" : "Azure";
        public override string DbConnectionString { get; } = CloudConfigurationManager.GetSetting("DBConnectionString");
    }
}