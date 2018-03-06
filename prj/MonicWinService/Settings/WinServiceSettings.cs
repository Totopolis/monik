using System.Collections.Generic;
using System.Configuration;

namespace Monik.Service
{
    public class WinServiceSettings : ServiceSettings
    {
        public override string InstanceName       { get; } = "Dev";
        public override string DbConnectionString { get; } = ConfigurationManager.AppSettings["DBConnectionString"];
    }
}