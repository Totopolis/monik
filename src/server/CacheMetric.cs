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

        private readonly Dictionary<int, Dictionary<string, IMetricObject>> _instanceMetricMap;

        public CacheMetric(IRepository repository, ILifetimeScope autofac, IMonik monik)
        {
            _repository = repository;
            _autofac = autofac;
            _monik = monik;

            _instanceMetricMap = new Dictionary<int, Dictionary<string, IMetricObject>>();

            _monik.ApplicationVerbose("CacheMetric.ctor");
        }

        public void OnStart()
        {
            _monik.ApplicationVerbose("CacheMetric.OnStart");

            // Load metrics and measures from db
        }

        public void OnStop()
        {
            // nothing
        }

        private IMetricObject CheckMetric(Instance instance, string metricName)
        {
            if (!_instanceMetricMap.ContainsKey(instance.ID))
                return null;

            var metrics = _instanceMetricMap[instance.ID];

            if (!metrics.ContainsKey(metricName))
                return null;

            return metrics[metricName];
        }

        public void OnNewMeasure(Instance instance, Event metric)
        {
            var metricObj = CheckMetric(instance, metric.Mc.Name);

            // TODO: if (metricObj != null && metricObj.Aggregation != metric.Mc.Aggregation)
            // then recreate

            if (metricObj == null)
            {
                metricObj = _autofac.Resolve<IMetricObject>();

                metricObj.CreateNew(
                    metric.Mc.Name,
                    (int)metric.Mc.Aggregation,
                    instance);
            }

            metricObj.OnNewMeasure(metric);
        }
    } //end of class
}
