using System.Collections.Generic;

namespace Monik.Service
{
    public interface IMonikServiceSettings
    {
        void UpdateSettings(Dictionary<string, string> val);

        string DbConnectionString { get; }
        string InstanceName { get; }

        string OutcomingConnectionString { get; }
        string OutcomingQueue { get; }
        int DayDeepLog { get; }
        int DayDeepKeepAlive { get; }

        int CleanupBatchSize { get; }
        int WriteBatchSize { get; }
        int WriteBatchTimeout { get; }

        string AuthSecretKey { get; }
    }
}