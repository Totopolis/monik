using System.Collections.Generic;
using Microsoft.Azure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Monik.Service
{
    public class CloudServiceSettings : ServiceSettings
    {
       public override string InstanceName
        {
            get { return RoleEnvironment.IsEmulated ? "Dev" : "Azure"; }
        }
        
        public override string DbConnectionString
        {
            get { return CloudConfigurationManager.GetSetting("DBConnectionString"); }
        }
    }
}