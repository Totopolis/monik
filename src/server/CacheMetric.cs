using System;
using System.Collections.Generic;
using System.Linq;
using Monik.Common;
using System.Collections.Concurrent;
using Autofac;

namespace Monik.Service
{
    public class CacheMetric : ICacheMetric
    {
        private readonly IRepository _repository;
        private readonly ILifetimeScope _autofac;
        private readonly IMonik _monik;

        private readonly Scheduler _sheduler;
        private readonly ConcurrentBag<IMetricObject> _metrics;

        public CacheMetric(IRepository repository, ILifetimeScope autofac, IMonik monik)
        {
            _repository = repository;
            _autofac = autofac;
            _monik = monik;

            _sheduler = Scheduler.CreatePerMinute(monik, this.BackgroundIntervalPush, "CacheMetric.BackgroundIntervalPush");
            _metrics = new ConcurrentBag<IMetricObject>();

            _monik.ApplicationVerbose("CacheMetric.ctor");
        }

        public void OnStart()
        {
            _monik.ApplicationVerbose("CacheMetric.OnStart");

            // Load metrics to instances and measures to metricobjects, from db

            foreach (var mo in _metrics.ToArray())
                mo.OnStart();

            _sheduler.OnStart();
        }

        public void OnStop()
        {
            _sheduler.OnStop();

            foreach (var mo in _metrics.ToArray())
                mo.OnStop();
        }

        private void BackgroundIntervalPush()
        {
            foreach (var mo in _metrics.ToArray())
                mo.BackgroundIntervalPush();
        }

        public void OnNewMeasure(Instance instance, Event metric)
        {
            var metName = metric.Mc.Name;

            var metObj = instance.Metrics.ContainsKey(metName) ?
                instance.Metrics[metName] : null;

            // TODO: if (metricObj != null && metricObj.Aggregation != metric.Mc.Aggregation)
            // then recreate

            if (metObj == null)
            {
                metObj = _autofac.Resolve<IMetricObject>();

                metObj.CreateNew(
                    metName,
                    (int)metric.Mc.Aggregation,
                    instance);

                instance.Metrics.Add(metName, metObj);
                _metrics.Add(metObj);
            }

            metObj.OnNewMeasure(metric);
        }
    } //end of class
}
