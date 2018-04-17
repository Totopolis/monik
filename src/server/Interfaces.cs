using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Monik.Common;

namespace Monik.Service
{
    public interface IObject
    {
        void OnStart();
        void OnStop();
    }

    public interface IMonikServiceSettings : IObject
    {
        string DbConnectionString { get; }
        string InstanceName { get; }

        string OutcomingConnectionString { get; }
        string OutcomingQueue { get; }
        int DayDeepLog { get; }
        int DayDeepKeepAlive { get; }
    }

    public class EventQueue
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public byte Type { get; set; }
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

        void CreateKeepAlive(KeepAlive_ keepAlive);
        void CreateLog(Log_ log);

        List<EventQueue> GetEventSources();

        Metric_ CreateMetric(string name, int aggregation, int instanceId);
        Measure_[] GetMeasures(int metricId);

        // DeleteMetric(int id) ... also delete measures
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

        bool IsDefaultInstance(int instance);
        bool IsInstanceInGroup(int instanceId, short groupId);
    }

    public interface ICacheLog : IObject
    {
        long LastLogId { get; }
        void OnNewLog(Log_ log);

        List<Log_> GetLogs5(LogRequest filter);
    }

    public interface ICacheKeepAlive : IObject
    {
        long LastKeepAliveId { get; }
        void OnNewKeepAlive(KeepAlive_ keepAlive);

        List<KeepAlive_> GetKeepAlive2(KeepAliveRequest filter);
    }

    public interface ICacheMetric : IObject
    {
        void OnNewMeasure(Instance instance, Event metric);
    }

    public interface IMessagePump : IObject
    {
        void OnEmbeddedEvents(ConcurrentQueue<Event> events);
    }

    public interface IMessageProcessor : IObject
    {
        void Process(Event ev, Instance instance);
    }

    public interface IMetricObject : IObject
    {
        void CreateNew(string name, int aggregation, Instance instance);
        void Load(int metricId);

        void OnNewMeasure(Event metric);

        void BackgroundIntervalPush();
    }
}
