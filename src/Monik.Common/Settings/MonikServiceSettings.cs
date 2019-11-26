using System.Configuration;

namespace Monik.Service
{
    public class MonikServiceSettings : IMonikServiceSettings
    {
        public DbProvider DbProvider =>
            (DbProvider) System.Enum.Parse(typeof(DbProvider), ConfigurationManager.AppSettings["DBProvider"]);
        public string InstanceName => ConfigurationManager.AppSettings["InstanceName"];

        public string DbConnectionString => ConfigurationManager.AppSettings["DBConnectionString"];

        public int DayDeepKeepAlive => int.Parse(ConfigurationManager.AppSettings["DayDeepKeepAlive"]);
        public int DayDeepLog => int.Parse(ConfigurationManager.AppSettings["DayDeepLog"]);
        public int CleanupBatchSize => int.Parse(ConfigurationManager.AppSettings["CleanupBatchSize"]);
        
        public int WriteBatchSize => int.Parse(ConfigurationManager.AppSettings["WriteBatchSize"]);
        public int WriteBatchTimeout => int.Parse(ConfigurationManager.AppSettings["WriteBatchTimeout"]);

        public string AuthSecretKey => ConfigurationManager.AppSettings["AuthSecretKey"];
    } //end of class
}
