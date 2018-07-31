using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Monik.Common;
using Autofac;

namespace Monik.Service
{
    public class CacheMetric : ICacheMetric
    {
        private readonly IRepository _repository;
        private readonly ILifetimeScope _autofac;
        private readonly ISourceInstanceCache _sourceCache;
        private readonly IMonik _monik;

        private readonly Scheduler _shedulerPerMin;
        private readonly Scheduler _shedulerPerSec;
        private readonly ConcurrentBag<IMetricObject> _metrics;

        public CacheMetric(IRepository repository, ILifetimeScope autofac,
            ISourceInstanceCache sourceCache, IMonik monik)
        {
            _repository = repository;
            _autofac = autofac;
            _sourceCache = sourceCache;
            _monik = monik;

            _shedulerPerMin = Scheduler.CreatePerMinute(monik, 
                this.BackgroundIntervalPush, "CacheMetric.BackgroundIntervalPush");

            _shedulerPerSec = Scheduler.CreatePerSecond(monik,
                this.BackgroundSecondPush, "CacheMetric.BackgroundSecondPush");

            _metrics = new ConcurrentBag<IMetricObject>();

            _monik.ApplicationVerbose("CacheMetric.ctor");
        }

        public Metric_[] GetMetricsDescriptions() => _metrics.Select(x => x.Dto).ToArray();

        public MeasureResponse GetCurrentMeasure(int metricId)
        {
            var metricSearch = _metrics.Where(x => x.Dto.ID == metricId);

            if (metricSearch.Count() == 0)
                throw new Exception($"Metric {metricId} not found.");

            var metric = metricSearch.First();
            return metric.GetCurrentMeasure();
        }

        public MeasureResponse[] GetAllCurrentMeasures()
        {
            var measures = _metrics.Select(x => x.GetCurrentMeasure());
            return measures.ToArray();
        }

        public WindowResponse[] GetAllWindowsMeasures()
        {
            var windows = _metrics.Select(x => x.GetWindow());
            return windows.ToArray();
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
                instance.Metrics.TryAdd(metObj.Dto.Name, metObj);

                _metrics.Add(metObj);
            }

            foreach (var mo in _metrics.ToArray())
                mo.OnStart();

            _shedulerPerMin.OnStart();
            _shedulerPerSec.OnStart();
        }

        public void OnStop()
        {
            _shedulerPerSec.OnStop();
            _shedulerPerMin.OnStop();

            foreach (var mo in _metrics.ToArray())
                mo.OnStop();
        }

        private void BackgroundIntervalPush()
        {
            foreach (var mo in _metrics.ToArray())
                mo.BackgroundIntervalPush();
        }

        private void BackgroundSecondPush()
        {
            foreach (var mo in _metrics.ToArray())
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

                _metrics.Add(metObj);
            }

            // TODO: if (metricObj != null && metricObj.Aggregation != metric.Mc.Aggregation)
            // then recreate

            metObj.OnNewMeasure(metric);
        }
    } //end of class
}
