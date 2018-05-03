using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        public Metric_ Dto => _dto;

        public MetricObject(IMonik monik, IRepository repository)
        {
            _monik = monik;
            _repository = repository;

            _dto = null;
            _measures = null;
        }

        public MeasureResponse GetCurrentMeasure()
        {
            var actualIndx = _dto.ActualID - _dto.RangeHeadID;
            var actualMeasure = _measures[actualIndx];

            return new MeasureResponse
            {
                MetricId = _dto.ID,
                Interval = _dto.ActualInterval,
                Value = actualMeasure.Value
            };
        }

        private Measure_ GetMeasure(long intervalId)
        {
            var actualIndx = intervalId - _dto.RangeHeadID;
            return _measures[actualIndx];
        }
        
        public void CreateNew(string name, int aggregation, Instance instance)
        {
            _dto = _repository.CreateMetric(name, aggregation, instance.ID);
            _measures = _repository.GetMeasures(_dto.ID);
        }

        public void Load(int metricId)
        {
            _dto = _repository.GetMetric(metricId);
            _measures = _repository.GetMeasures(metricId);
        }

        public void OnNewMeasure(Event metric)
        {
            var metTime = Helper.FromMillisecondsSinceUnixEpoch(metric.Created);

            var actualIntervalEnd = _dto.ActualInterval;
            var actualIntervalStart = _dto.ActualInterval.AddMinutes(-5);
            
            if (metTime < actualIntervalStart || metTime >= actualIntervalEnd)
            {
                // skip event
                // increase skip metric

                return;
            }

            var actualMeasure = GetMeasure(_dto.ActualID);

            switch (metric.Mc.Aggregation)
            {
                case AggregationType.Accumulator:
                    actualMeasure.Value += metric.Mc.Value;
                    break;
                case AggregationType.Gauge:
                    actualMeasure.Value = (actualMeasure.Value + metric.Mc.Value) / 2;
                    break;
                default:
                    // skip event
                    // increase skip metric
                    break;
            }
        }

        // TODO: lock sections

        public void BackgroundIntervalPush()
        {
            var curInterval = GetCurrentIntervalEnd();

            List<long> _intervalsToSave = new List<long>();

            while (curInterval > _dto.ActualInterval)
            {
                _dto.ActualInterval += TimeSpan.FromMinutes(5);

                _intervalsToSave.Add(_dto.ActualID);

                _dto.ActualID = _dto.ActualID == _dto.RangeTailID ?
                    _dto.RangeHeadID :
                    (_dto.ActualID + 1);
            }

            if (_intervalsToSave.Count > 0)
            {
                var measures = _intervalsToSave
                    .Select(x => GetMeasure(x))
                    .ToArray();

                _repository.SaveMetric(_dto, measures);
            }
        }

        public void OnStart()
        {
            // nothibg
        }

        public void OnStop()
        {
            // TODO: save last measure and metric state
        }

        private DateTime GetCurrentIntervalEnd() => DateTime.UtcNow.RoundUp(TimeSpan.FromMinutes(5));
    }//end of class
}
