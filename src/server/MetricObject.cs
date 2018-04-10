using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Autofac;
using Monik.Common;

namespace Monik.Service
{
    public interface IMetricObject
    {
        void CreateNew(string name, int aggregation, Instance instance);
        void Load(int metricId);

        void OnNewMeasure(Event metric);
    }

    public class MetricObject : IMetricObject
    {
        private readonly IMonik _monik;
        private readonly IRepository _repository;

        private Metric_ _dto;
        private Measure_[] _measures;

        public MetricObject(IMonik monik, IRepository repository)
        {
            _monik = monik;
            _repository = repository;

            _dto = null;
            _measures = null;
        }

        public void CreateNew(string name, int aggregation, Instance instance)
        {
            _dto = _repository.CreateMetric(name, aggregation, instance.ID);
            _measures = _repository.GetMeasures(_dto.Id);
        }

        public void Load(int metricId)
        {
            throw new NotImplementedException();

            // _dto = _repository.GetMetric(metricId);
            // _measures = _repository.GetMeasures(metricId);
        }

        public void OnNewMeasure(Event metric)
        {
            
        }
    }//end of class
}
