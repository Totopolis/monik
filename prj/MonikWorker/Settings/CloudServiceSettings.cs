using System.Collections.Generic;
using Microsoft.Azure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Monik.Service
{
    public class CloudServiceSettings : ServiceSettings
    {
        public override string InstanceName       { get; } = RoleEnvironment.IsEmulated ? "Dev" : "Azure";
        public override string DbConnectionString { get; } = CloudConfigurationManager.GetSetting("DBConnectionString");
    }
}