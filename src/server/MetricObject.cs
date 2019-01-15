using Monik.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Monik.Service
{
    // server methods (ala app insignt): total request, failed request (<> http 200), average response time
    // total exceptions

    public class MetricObject : IMetricObject
    {
        private const int IntervalEpsilon = 1000; // ms

        private readonly IMonik _monik;
        private readonly IRepository _repository;

        private Metric_ _dto;
        private Measure_[] _measures;

        public Metric_ Dto => _dto;

        private IWindowCalculator window;

        public MetricObject(IMonik monik, IRepository repository)
        {
            _monik = monik;
            _repository = repository;

            _dto = null;
            _measures = null;

            window = null;
        }

        public MeasureResponse GetCurrentMeasure()
        {
            lock (this)
            {
                var actualMeasure = GetMeasure(_dto.ActualID);

                return new MeasureResponse
                {
                    MetricId = _dto.ID,
                    Interval = _dto.ActualInterval,
                    Value = actualMeasure.Value
                };
            }
        }

        public WindowResponse GetWindow()
        {
            return new WindowResponse
            {
                MetricId = _dto.ID,
                Value = window == null ? 0 : window.GetValue()
            };
        }

        public MetricHistoryResponse GetMetricHistory(int amount, int skip)
        {
            var interval = _dto.ActualInterval.AddMinutes(-5 * skip);
            var historyMeasures =
                GetHistoryValuesEnumerable(skip)
                    .Take(amount)
                    .ToArray();

            return new MetricHistoryResponse
            {
                MetricId = _dto.ID,
                Interval = interval,
                Values = historyMeasures
            };
        }

        private IEnumerable<double> GetHistoryValuesEnumerable(int skip)
        {
            var dif = _dto.RangeTailID - _dto.RangeHeadID;
            var actualIdx = _dto.ActualID - _dto.RangeHeadID;
            var i = skip;
            while (i < int.MaxValue)
            {
                var index = (actualIdx - i++) % dif;
                if (index < 0)
                    index += dif;
                yield return _measures[index].Value;
            }
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

            lock (this)
            {
                var actualIntervalStart = _dto.ActualInterval.AddMinutes(-5);
                var actualIntervalEnd = _dto.ActualInterval;
                
                // interval with epsilon
                var intervalStart = actualIntervalStart.AddMilliseconds(-IntervalEpsilon);
                var intervalEnd = actualIntervalEnd.AddMilliseconds(IntervalEpsilon);

                if (metTime < intervalStart || metTime > intervalEnd)
                {
                    // skip event
                    // increase skip metric
                    _monik.Measure("OutTimeMeasure", AggregationType.Accumulator, 1);
                    var serverTime = DateTime.UtcNow;
                    var diffInterval = metTime < intervalStart
                        ? (metTime - actualIntervalStart).TotalMilliseconds
                        : (metTime - actualIntervalEnd).TotalMilliseconds;
                    var diffServer = (serverTime - metTime).TotalMilliseconds;
                    _monik.LogicVerbose($@"[OutTime] {metric.Source}.{metric.Instance}::{metric.Mc.Name}, lag:{diffServer}, lagInterval:{diffInterval}, {metric.Created} not in [{new DateTimeOffset(actualIntervalStart).ToUnixTimeMilliseconds()},{new DateTimeOffset(actualIntervalEnd).ToUnixTimeMilliseconds()}), now:{new DateTimeOffset(serverTime).ToUnixTimeMilliseconds()}");
                    return;
                }

                var actualMeasure = GetMeasure(_dto.ActualID);

                switch (metric.Mc.Aggregation)
                {
                    case AggregationType.Accumulator:
                        if (actualMeasure.HasValue)
                            actualMeasure.Value += metric.Mc.Value;
                        else
                        {
                            actualMeasure.Value = metric.Mc.Value;
                            actualMeasure.HasValue = true;
                        }

                        AccumWindowCalculator accWin = window as AccumWindowCalculator;
                        if (accWin == null)
                            window = new AccumWindowCalculator();

                        window.OnNewValue(metric.Mc.Value);
                        break;

                    case AggregationType.Gauge:
                        if (actualMeasure.HasValue)
                            actualMeasure.Value = (actualMeasure.Value + metric.Mc.Value) / 2;
                        else
                        {
                            actualMeasure.Value = metric.Mc.Value;
                            actualMeasure.HasValue = true;
                        }

                        GaugeWindowCalculator gauWin = window as GaugeWindowCalculator;
                        if (gauWin == null)
                            window = new GaugeWindowCalculator();

                        window.OnNewValue(metric.Mc.Value);
                        break;

                    default:
                        // skip event
                        // increase skip metric
                        break;
                }
            }//lock
        }

        public void BackgroundIntervalPush()
        {
            var curInterval = GetCurrentIntervalEnd();

            List<long> _intervalsToSave = new List<long>();

            lock (this)
            {
                while (curInterval > _dto.ActualInterval)
                {
                    _dto.ActualInterval += TimeSpan.FromMinutes(5);

                    _intervalsToSave.Add(_dto.ActualID);

                    _dto.ActualID = _dto.ActualID == _dto.RangeTailID ?
                        _dto.RangeHeadID :
                        (_dto.ActualID + 1);

                    // cleanup next current interval !!!!
                    var actualMeasure = GetMeasure(_dto.ActualID);
                    actualMeasure.Value = 0;
                    actualMeasure.HasValue = false;
                }

                if (_intervalsToSave.Count > 0)
                {
                    var measures = _intervalsToSave
                        .Select(x => GetMeasure(x))
                        .ToArray();

                    _repository.SaveMetric(_dto, measures);
                }
            }//lock
        }

        public void BackgroundSecondPush()
        {
            if (window != null)
                window.OnNextSecond();
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
