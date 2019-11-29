namespace Monik.Service
{
    public class MonikServiceSettings : IMonikServiceSettings
    {
        public DbProvider DbProvider { get; set; }
        public string InstanceName { get; set; }

        public string DbConnectionString { get; set; }

        public int DayDeepKeepAlive { get; set; }
        public int DayDeepLog { get; set; }
        public int CleanupBatchSize { get; set; }
        
        public int WriteBatchSize { get; set; }
        public int WriteBatchTimeout { get; set; }

        public string AuthSecretKey { get; set; }

        public QueueReaderSettings[] Readers { get; set; }
    } //end of class
}
