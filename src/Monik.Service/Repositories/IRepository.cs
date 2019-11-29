using System;
using System.Collections.Generic;

namespace Monik.Service
{
    public interface IRepository
    {
        List<Source> GetAllSources();
        List<Instance> GetAllInstances();
        List<Group> GetAllGroupsAndFill();

        void CreateNewSource(Source src);
        void CreateNewInstance(Instance ins);

        void AddInstanceToGroup(int iId, short gId);
        void RemoveInstanceFromGroup(int iId, short gId);
        short CreateGroup(Group_ group);
        void RemoveGroup(short id);

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
}