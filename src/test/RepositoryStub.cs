using Monik.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Monik.Service.Test
{
    public class RepositoryStub : IRepository
    {
        private readonly List<Source> _sourceList = new List<Source>();
        private readonly List<Instance> _instanceList = new List<Instance>();

        private long _logLastId = 0;
        private long _kaLastId = 0;

        public RepositoryStub() { }

        public List<Source> GetAllSources() => new List<Source>();

        public List<Instance> GetAllInstances() => new List<Instance>();

        public List<Group> GetAllGroupsAndFill() => new List<Group>();

        public void CreateNewSource(Source src)
        {
            short maxSourceId = _sourceList
                .Select(x => x.ID)
                .DefaultIfEmpty()
                .Max();

            maxSourceId++;

            _sourceList.Add(src);
            src.ID = maxSourceId;
        }

        public void CreateNewInstance(Instance ins)
        {
            int maxInstanceId = _instanceList
                .Select(x => x.ID)
                .DefaultIfEmpty()
                .Max();

            maxInstanceId++;

            _instanceList.Add(ins);
            ins.ID = maxInstanceId;
        }

        public void AddInstanceToGroup(Instance ins, Group group) => throw new NotImplementedException();

        public long GetMaxLogId() => _logLastId;

        public long GetMaxKeepAliveId() => _kaLastId;

        public List<Log_> GetLastLogs(int top) => new List<Log_>();

        public List<KeepAlive_> GetLastKeepAlive(int top) => new List<KeepAlive_>();

        public long? GetLogThreshold(int dayDeep) => 0;

        public long? GetKeepAliveThreshold(int dayDeep) => 0;

        public int CleanUpLog(long lastLog) => 0;

        public int CleanUpKeepAlive(long lastKeepAlive) => 0;

        public void CreateHourStat(DateTime hour, long lastLogId, long lastKeepAliveId) { }

        public void CreateKeepAlive(KeepAlive_ keepAlive)
        {
            _kaLastId++;
            keepAlive.ID = _kaLastId;
        }

        public void CreateLog(Log_ log)
        {
            _logLastId++;
            log.ID = _logLastId;
        }

        public List<EventQueue> GetEventSources() => new List<EventQueue>();

        private int _lastMetricId = 1;
        public const int NumMeasures = 100;
        private long _lastMeasureId = 1;

        private Dictionary<int, Metric_> _metrics = new Dictionary<int, Metric_>();

        public Metric_ CreateMetric(string name, int aggregation, int instanceId)
        {
            Metric_ obj = new Metric_
            {
                ID = _lastMetricId,
                Name = name,
                Aggregation = aggregation,
                InstanceID = instanceId,
                RangeHeadID = _lastMeasureId,
                RangeTailID = _lastMeasureId + NumMeasures - 1,
                ActualInterval = DateTime.UtcNow.RoundUp(TimeSpan.FromMinutes(5)),
                ActualID = _lastMeasureId
            };

            _lastMeasureId += NumMeasures;
            _lastMetricId++;

            _metrics.Add(obj.ID, obj);

            return obj;
        }

        public Measure_[] GetMeasures(int metricId)
        {
            var met = _metrics[metricId];

            var res = Enumerable.Range((int)met.RangeHeadID, NumMeasures)
                .Select(x => new Measure_() { ID = x, Value = 0 });

            return res.ToArray();
        }

        public void SaveMetric(Metric_ metric, Measure_[] measures)
        {
        }

        public Metric_ GetMetric(int metricId)
        {
            throw new NotImplementedException();
        }

        public int[] GetAllMetricIds()
        {
            return new int[0];
        }
    } //end of class
}
