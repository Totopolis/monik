using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Autofac;
using Monik.Common;

namespace Monik.Service
{
    // server methods (ala app insignt): total request, failed request (<> http 200), average response time
    // total exceptions

    public class MetricObject : IMetricObject
    {
        private readonly IMonik _monik;
        private readonly IRepository _repository;

        private Metric_ _dto;
        private Measure_[] _measures;

        // Need internal timer for push measure ticks

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
            var metTime = Helper.FromMillisecondsSinceUnixEpoch(metric.Created);

            var actualIntervalEnd = _dto.ActualIntervalTime;
            var actualIntervalStart = _dto.ActualIntervalTime.AddMinutes(-5);
            
            if (metTime < actualIntervalStart || metTime >= actualIntervalEnd)
            {
                // skip event
                // increase skip metric

                return;
            }

            var actualIndx = _dto.ActualId - _dto.RangeHeadId;
            var actualMeasure = _measures[actualIndx];

            switch (metric.Mc.Aggregation)
            {
                case AggregationType.Accumulator:
                    actualMeasure.Val += metric.Mc.Value;
                    break;
                case AggregationType.Gauge:
                    actualMeasure.Val = (actualMeasure.Val + metric.Mc.Value) / 2;
                    break;
                default:
                    // skip event
                    // increase skip metric
                    break;
            }
        }

        public void BackgroundIntervalPush()
        {
            var curInterval = GetCurrentIntervalEnd();

            List<long> _intervalsToSave = new List<long>();

            while (curInterval > _dto.ActualIntervalTime)
            {
                _dto.ActualIntervalTime += TimeSpan.FromMinutes(5);

                _intervalsToSave.Add(_dto.ActualId);

                _dto.ActualId = _dto.ActualId == _dto.RangeTailId ?
                    _dto.RangeHeadId :
                    _dto.ActualId++;
            }

            if (_intervalsToSave.Count > 0)
            {
                // TODO: save intervals
                // save dto
            }
        }

        public void OnStart()
        {
            // nothibg
        }

        public void OnStop()
        {
            // save last measure and metric state
        }

        private DateTime GetCurrentIntervalEnd() => DateTime.UtcNow.RoundUp(TimeSpan.FromMinutes(5));
    }//end of class
}
