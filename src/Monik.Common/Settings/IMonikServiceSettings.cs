﻿namespace Monik.Service
{
    public interface IMonikServiceSettings
    {
        string DbConnectionString { get; }
        string InstanceName { get; }

        int DayDeepLog { get; }
        int DayDeepKeepAlive { get; }
        int CleanupBatchSize { get; }

        int WriteBatchSize { get; }
        int WriteBatchTimeout { get; }

        string AuthSecretKey { get; }
    }
}