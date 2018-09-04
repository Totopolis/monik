using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Monik.Common;
using Nancy;
using Nancy.Security;

namespace Monik.Service
{
    public interface IObject
    {
        void OnStart();
        void OnStop();
    }

    public interface IUserIdentityProvider
    {
        IUserIdentity GetUserIdentity(NancyContext ctx);
    }

    public interface IMonikServiceSettings : IObject
    {
        string DbConnectionString { get; }
        string InstanceName { get; }

        string OutcomingConnectionString { get; }
        string OutcomingQueue { get; }
        int DayDeepLog { get; }
        int DayDeepKeepAlive { get; }

        int CleanupBatchSize { get; }
        int WriteBatchSize { get; }
        int WriteBatchTimeout { get; }

        string AuthSecretKeyBase64 { get; }
        byte[] AuthSecretKey { get; }
    }

    public enum EventQueueType : byte
    {
        AzureQueue = 1,
        RabbitQueue = 2,
        SqlQueue = 3
    }

    public class EventQueue
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public EventQueueType Type { get; set; }
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
    }

    public interface IRepository
    {
        List<Source> GetAllSources();
        List<Instance> GetAllInstances();
        List<Group> GetAllGroupsAndFill();

        void CreateNewSource(Source src);
        void CreateNewInstance(Instance ins);
        void AddInstanceToGroup(Instance ins, Group group);

        long GetMaxLogId();
        long GetMaxKeepAliveId();

        List<Log_> GetLastLogs(int top);
        List<KeepAlive_> GetLastKeepAlive(int top);

        long? GetLogThreshold(int dayDeep);
        long? GetKeepAliveThreshold(int dayDeep);

        int CleanUpLog(long lastLog);
        int CleanUpKeepAlive(long lastKeepAlive);

        void CreateHourStat(DateTime hour, long lastLogId, long lastKeepAliveId);

        void WriteKeepAlives(IEnumerable<KeepAlive_> values);
        void WriteLogs(IEnumerable<Log_> values);

        List<EventQueue> GetEventSources();

        int[] GetAllMetricIds();
        Metric_ CreateMetric(string name, int aggregation, int instanceId);
        Metric_ GetMetric(int metricId);
        Measure_[] GetMeasures(int metricId);

        void SaveMetric(Metric_ metric, Measure_[] measures);

        void RemoveMetric(int id);
        void RemoveInstance(int id);
        void RemoveSource(short id);

        // int[] GetAllMetrics()
        // Metric_ GetMetric(int id)
        // int CreateMetric(string name, int aggregation, int instanceId) ... also created range in Measure table
        // (long, double) GetMeasures(int metricId)
    }

    public interface ISourceInstanceCache : IObject
    {
        Instance CheckSourceAndInstance(string sourceName, string instanceName);
        Source GetSourceByInstanceId(int instanceId);
        Instance GetInstanceById(int instanceId);

        List<Instance> GetAllInstances();
        List<Source> GetAllSources();
        List<Group> GetAllGroups();

        void RemoveSource(short id);
        void RemoveInstance(int id);
        event Action<IEnumerable<int>> RemoveMetrics; 

        bool IsDefaultInstance(int instance);
        bool IsInstanceInGroup(int instanceId, short groupId);
    }

    public interface ICacheEntity
    {
        long ID { get; set; }
    }

    public interface ICacheBase<in TEntity> : IObject where TEntity : ICacheEntity
    {
        void Flush();
        void Add(TEntity entity);
        long LastId { get; }
    }

    public interface ICacheLog : ICacheBase<Log_>
    {
        List<Log_> GetLogs5(LogRequest filter);
    }

    public interface ICacheKeepAlive : ICacheBase<KeepAlive_>
    {
        List<KeepAlive_> GetKeepAlive2(KeepAliveRequest filter);
    }

    public interface ICacheMetric : IObject
    {
        void OnNewMeasure(Instance instance, Event metric);

        void RemoveMetric(int id);

        Metric_[] GetMetricsDescriptions();
        MeasureResponse GetCurrentMeasure(int metricId);
        MeasureResponse[] GetAllCurrentMeasures();
        WindowResponse[] GetAllWindowsMeasures();
    }

    public interface IMessagePump : IObject
    {
        void OnEmbeddedEvents(ConcurrentQueue<Event> events);
    }

    public interface IMessageProcessor : IObject
    {
        void Process(Event ev, Instance instance);
        void FinalizeProcessing();
    }

    public interface IMetricObject : IObject
    {
        Metric_ Dto { get; }

        MeasureResponse GetCurrentMeasure();
        WindowResponse GetWindow();

        void CreateNew(string name, int aggregation, Instance instance);
        void Load(int metricId);

        void OnNewMeasure(Event metric);

        void BackgroundIntervalPush();
        void BackgroundSecondPush();
    }
}
