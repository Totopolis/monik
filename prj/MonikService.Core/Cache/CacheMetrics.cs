using System;
using System.Collections.Generic;
using System.Linq;
using Monik.Client;

namespace Monik.Service
{
    public class CacheMetrics : ICacheMetrics
    {
        private readonly IRepository _repository;
        private readonly ISourceInstanceCache _cache;
        private readonly IClientControl _control;
        private readonly IServiceSettings _settings;
        
        private readonly Dictionary<int, MetricDescription> metricDescriptions;
        private readonly Dictionary<ValueTuple<int, string>, MetricDescription> metricDescriptionsMap;
        private readonly Dictionary<int, Queue<MetricValue>> metricValues;
        private readonly Dictionary<int, List<int>> metricAggregatingValues;

        private int aggregatingValuesCapacity = 10;

        public CacheMetrics(IRepository aRepository, ISourceInstanceCache aCache, IClientControl aControl, IServiceSettings settings)
        {
            _repository = aRepository;
            _cache = aCache;
            _control = aControl;
            _settings = settings;


            // fill from database
            var _descriptions = _repository.GetMetricDescriptions();
            
            metricDescriptionsMap = _descriptions.ToDictionary(d => new ValueTuple<int, string>(d.InstanceID, d.Name));

            metricDescriptions = _descriptions.ToDictionary(d => d.Id);

            var stubsCount = settings.MetricDeepMinutes * 60 / settings.MetricInterval;

            foreach (var mDesc in metricDescriptions.Values)
            {
                var tmp = stubsCount - mDesc.SavedValuesCount;
                if (tmp > 0)
                    _repository.AddMetricValueStubs(tmp, mDesc.Id);
                if (tmp < 0)
                    _repository.DeleteMetricValueStubs(-tmp, mDesc.Id);
            }

            metricValues = _repository.GetAllMetricValues().GroupBy(mv=>mv.MetricId).ToDictionary(g=>g.Key, g=> new Queue<MetricValue>(g));

            metricAggregatingValues = metricDescriptions.Keys.ToDictionary(md => md, _ => new List<int>(aggregatingValuesCapacity));

            _control.ApplicationVerbose($"{nameof(CacheMetrics)} created");
        }
        
        public void OnStart()
        {
            _control.ApplicationVerbose($"{nameof(CacheMetrics)} started");
        }

        public void OnStop()
        {
            _control.ApplicationVerbose($"{nameof(CacheMetrics)} stopped");
        }
    }
}