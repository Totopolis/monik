namespace MonikService.Core.Settings
{
    public interface IServiceSettings
    {
        string DbConnectionString { get; }
        string InstanceName       { get; }

        string OutcomingConnectionString { get; }
        string OutcomingQueue            { get; }
        int    DayDeepLog                { get; }
        int    DayDeepKeepAlive          { get; }
        int    MetricDeepMinutes         { get; }
        int    MetricInterval            { get; }
    }
}