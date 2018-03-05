using System.Collections.Generic;
using System.Configuration;

namespace Monik.Service
{
    public class WinServiceSettings : ServiceSettings
    {
        public override string InstanceName       => "Dev";
        public override string DbConnectionString => ConfigurationManager.AppSettings["DBConnectionString"];
    }
}