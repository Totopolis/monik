using Autofac;
using Monik.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Monik.Service
{
    public class CacheMetric : ICacheMetric
    {
        private readonly IRepository _repository;
        private readonly ILifetimeScope _autofac;
        private readonly ICacheSourceInstance _sourceCache;
        private readonly IMonik _monik;

        private readonly Scheduler _shedulerPerMin;
        private readonly Scheduler _shedulerPerSec;
        private readonly ConcurrentDictionary<IMetricObject, byte> _metrics; // as HashSet, byte value is not used

        public CacheMetric(IRepository repository, ILifetimeScope autofac,
            ICacheSourceInstance sourceCache, IMonik monik)
        {
            _repository = repository;
            _autofac = autofac;
            _sourceCache = sourceCache;
            _monik = monik;

            _shedulerPerMin = Scheduler.CreatePerMinute(monik, 
                this.BackgroundIntervalPush, "CacheMetric.BackgroundIntervalPush");

            _shedulerPerSec = Scheduler.CreatePerSecond(monik,
                this.BackgroundSecondPush, "CacheMetric.BackgroundSecondPush");

            _metrics = new ConcurrentDictionary<IMetricObject, byte>();

            _sourceCache.RemoveMetrics += OnRemoveMetrics;

            _monik.ApplicationVerbose("CacheMetric.ctor");
        }

        private void OnRemoveMetrics(IEnumerable<int> metIds)
        {
            foreach (var id in metIds)
                RemoveMetric(id);
        }

        public void RemoveMetric(int id)
        {
            // clean cache
            foreach (var item in _metrics.Keys.Where(x => x.Dto.ID == id).ToList())
            {
                item.OnStop();

                var instance = _sourceCache.GetInstanceById(item.Dto.InstanceID);
                instance?.Metrics.TryRemove(item.Dto.Name, out _);

                _metrics.TryRemove(item, out _);
            }

            // repository
            _repository.RemoveMetric(id);
        }

        public Metric_[] GetMetricsDescriptions() => _metrics.Keys.Select(x => x.Dto).ToArray();

        private IEnumerable<IMetricObject> GetMetricObjectsByFilter(MetricRequest filter)
        {
            if (filter.Groups.Length == 0 && filter.Instances.Length == 0)
            {
                return _metrics.Keys
                    .Where(m => _sourceCache.IsDefaultInstance(m.Dto.InstanceID));
            }
            else
            {
                return _metrics.Keys
                    .Where(m =>
                        filter.Groups.Any(gr => _sourceCache.IsInstanceInGroup(m.Dto.InstanceID, gr)) ||
                        filter.Instances.Contains(m.Dto.InstanceID));
            }
        }

        public MeasureResponse GetCurrentMeasure(int metricId)
        {
            var metricSearch = _metrics.Keys.Where(x => x.Dto.ID == metricId).ToList();

            if (!metricSearch.Any())
                throw new Exception($"Metric {metricId} not found.");

            var metric = metricSearch.First();
            return metric.GetCurrentMeasure();
        }

        public MeasureResponse[] GetCurrentMeasures(MetricRequest filter)
        {
            return GetMetricObjectsByFilter(filter)
                .Select(m => m.GetCurrentMeasure())
                .ToArray();
        }

        public MeasureResponse[] GetAllCurrentMeasures()
        {
            var measures = _metrics.Keys.Select(x => x.GetCurrentMeasure());
            return measures.ToArray();
        }

        public WindowResponse GetWindowMeasure(int metricId)
        {
            var metricSearch = _metrics.Keys.Where(x => x.Dto.ID == metricId).ToList();

            if (!metricSearch.Any())
                throw new Exception($"Metric {metricId} not found.");

            var metric = metricSearch.First();
            return metric.GetWindow();
        }

        public WindowResponse[] GetWindowMeasures(MetricRequest filter)
        {
            return GetMetricObjectsByFilter(filter)
                .Select(m => m.GetWindow())
                .ToArray();
        }

        public WindowResponse[] GetAllWindowsMeasures()
        {
            var windows = _metrics.Keys.Select(x => x.GetWindow());
            return windows.ToArray();
        }

        public MetricHistoryResponse GetMetricHistory(int metricId, int amount, int skip)
        {
            var metricSearch = _metrics.Keys.Where(x => x.Dto.ID == metricId).ToList();

            if (!metricSearch.Any())
                throw new Exception($"Metric {metricId} not found.");

            var metric = metricSearch.First();
            return metric.GetMetricHistory(amount, skip);
        }

        public void OnStart()
        {
            _monik.ApplicationVerbose("CacheMetric.OnStart");

            // Load metrics to instances and measures to metricobjects, from db

            foreach (var metId in _repository.GetAllMetricIds())
            {
                var metObj = _autofac.Resolve<IMetricObject>();

                metObj.Load(metId);

                var instance = _sourceCache.GetInstanceById(metObj.Dto.InstanceID);
                instance?.Metrics.TryAdd(metObj.Dto.Name, metObj);

                _metrics.TryAdd(metObj, 0);
            }

            foreach (var mo in _metrics.Keys)
                mo.OnStart();

            _shedulerPerMin.OnStart();
            _shedulerPerSec.OnStart();
        }

        public void OnStop()
        {
            _shedulerPerSec.OnStop();
            _shedulerPerMin.OnStop();

            foreach (var mo in _metrics.Keys)
                mo.OnStop();
        }

        private void BackgroundIntervalPush()
        {
            foreach (var mo in _metrics.Keys)
                mo.BackgroundIntervalPush();
        }

        private void BackgroundSecondPush()
        {
            foreach (var mo in _metrics.Keys)
                mo.BackgroundSecondPush();
        }

        public void OnNewMeasure(Instance instance, Event metric)
        {
            var metName = metric.Mc.Name;
            var metDic = instance.Metrics;

            if (!metDic.TryGetValue(metName, out var metObj))
            {
                metObj = _autofac.Resolve<IMetricObject>();

                // TODO: lock section (safe repository by create metric and measures)

                metObj.CreateNew(
                    metName,
                    (int)metric.Mc.Aggregation,
                    instance);

                metDic.TryAdd(metName, metObj);
                // TODO: what if false? may be already added?

                _metrics.TryAdd(metObj, 0);
            }

            // TODO: if (metricObj != null && metricObj.Aggregation != metric.Mc.Aggregation)
            // then recreate

            metObj.OnNewMeasure(metric);
        }
    } //end of class
}
