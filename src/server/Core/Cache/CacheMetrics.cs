using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Monik.Client.ClientControl;
using Monik.Common;
using MonikService.Core.Repository;
using MonikService.Core.Settings;

namespace MonikService.Core.Cache
{
    public class CacheMetrics : ICacheMetrics
    {
        private readonly IRepository          _repository;
        private readonly ISourceInstanceCache _sourceInstanceCache;
        private readonly IClientControl       _control;
        private readonly IServiceSettings     _settings;

        private readonly ConcurrentDictionary<long, MetricDescription>                     _metricDescriptions      = new ConcurrentDictionary<long, MetricDescription>();
        private readonly ConcurrentDictionary<ValueTuple<int, string>, MetricDescription>  _metricDescriptionsMap   = new ConcurrentDictionary<(int, string), MetricDescription>();
        private readonly ConcurrentDictionary<long, CircularBuffer<DateTime, MetricValue>> _metricValues            = new ConcurrentDictionary<long, CircularBuffer<DateTime, MetricValue>>();
        private readonly ConcurrentDictionary<long, AggMetricVal>                          _metricAggregatingValues = new ConcurrentDictionary<long, AggMetricVal>();

        private readonly int                                _stubsCount;
        private readonly ConcurrentDictionary<long, object> _metricValuesLockers = new ConcurrentDictionary<long, object>();
        private readonly CancellationTokenSource            _tokenSource         = new CancellationTokenSource();
        private          DateTime                           _timeWindowBegin;
        private readonly TimeSpan                           _metricInterval;

        public CacheMetrics(IRepository aRepository, ISourceInstanceCache aCache, IClientControl aControl, IServiceSettings settings)
        {
            _repository          = aRepository;
            _sourceInstanceCache = aCache;
            _control             = aControl;
            _settings            = settings;


            // fill from database
            _stubsCount = _settings.MetricDeepMinutes * 60 / _settings.MetricInterval;

            var values = _repository
               .GetAllMetricValues()
               .GroupBy(mv => mv.MetricId)
               .ToDictionary(g => g.Key, g => g.OrderBy(mv => mv.Created).ToList());

            _metricInterval = TimeSpan.FromSeconds(_settings.MetricInterval);

            _timeWindowBegin = DateTime.UtcNow;

            _timeWindowBegin = _timeWindowBegin.Add(-_timeWindowBegin.TimeOfDay +
                                                    TimeSpan.FromSeconds((int) _timeWindowBegin.TimeOfDay.TotalSeconds / _settings.MetricInterval * _settings.MetricInterval));

            if (values.Any() && _stubsCount != values.First().Value.Count)
            {
                // deleting metrics
                _repository.DeleteAllMetricStubs();
                _repository.DeleteAllMetricDescriptions();
            }
            else
            {
                var descriptions = _repository.GetMetricDescriptions();

                _metricDescriptionsMap =
                    new ConcurrentDictionary<(int, string), MetricDescription>(descriptions.ToDictionary(d => new ValueTuple<int, string>(d.InstanceId, d.Name)));

                _metricDescriptions = new ConcurrentDictionary<long, MetricDescription>(descriptions.ToDictionary(d => d.Id));

                //Update Stubs in DB
                foreach (var mValues in values)
                {
                    var popTime = _timeWindowBegin;

                    int upldateI;

                    for (upldateI = 0; popTime > mValues.Value.Last().Created; upldateI++, popTime -= _metricInterval)
                    {
                        var mV = mValues.Value[upldateI];

                        mV.Created        = popTime;
                        mV.Value          = 0;
                        mV.AggValuesCount = 0;

                        _repository.UpdateMetricValue(mV);
                    }
                }

                _metricValues = new ConcurrentDictionary<long, CircularBuffer<DateTime, MetricValue>>(
                    values.Select(v => new KeyValuePair<long, CircularBuffer<DateTime, MetricValue>>(
                                      v.Key, new CircularBuffer<DateTime, MetricValue>(v.Value.OrderBy(mv => mv.Created).ToList(), mv => mv.Created))));

                _metricAggregatingValues = new ConcurrentDictionary<long, AggMetricVal>(
                    _metricDescriptions.Keys.ToDictionary(md => md, _ => new AggMetricVal()));
            }

            _control.ApplicationVerbose($"{nameof(CacheMetrics)} created");

            Task.Run(() => AggregateTask(_tokenSource.Token), _tokenSource.Token);
        }

        public void AddMetricAggregatingValue(MetricDescription metricDesc, MetricValue value)
        {
            if (_metricDescriptionsMap.ContainsKey((metricDesc.InstanceId, metricDesc.Name)))
                metricDesc = _metricDescriptionsMap[(metricDesc.InstanceId, metricDesc.Name)];
            else
            {
                var locker = _metricValuesLockers.GetOrAdd(-1, new object());

                lock (locker)
                {
                    if (_metricDescriptionsMap.ContainsKey((metricDesc.InstanceId, metricDesc.Name)))
                        metricDesc = _metricDescriptionsMap[(metricDesc.InstanceId, metricDesc.Name)];
                    else
                    {
                        _repository.CreateMetricDescription(metricDesc);
                        _metricDescriptions.TryAdd(metricDesc.Id, metricDesc);
                        _metricDescriptionsMap.TryAdd((metricDesc.InstanceId, metricDesc.Name), metricDesc);

                        var stabsToAdd = new List<MetricValue>(_stubsCount);
                        var popTime    = _timeWindowBegin;

                        for (var i = _stubsCount - 1; i >= 0; i--, popTime -= _metricInterval)
                            stabsToAdd.Add(new MetricValue()
                            {
                                AggValuesCount = 0,
                                Created        = popTime,
                                MetricId       = metricDesc.Id,
                                Value          = 0
                            });

                        _repository.AddMetricValueStubs(stabsToAdd);
                        _metricValues.TryAdd(metricDesc.Id, new CircularBuffer<DateTime, MetricValue>(stabsToAdd, mv => mv.Created));
                        _metricAggregatingValues.TryAdd(metricDesc.Id, new AggMetricVal());
                    }
                }
            }


            if (value.Created > _metricValues[metricDesc.Id].Last)
            {
                var locker = _metricValuesLockers.GetOrAdd(metricDesc.Id, new object());

                lock (locker)
                {
                    _metricAggregatingValues[metricDesc.Id].AggsCount++;
                    _metricAggregatingValues[metricDesc.Id].Value += value.Value;
                }
            }
            else
            {
                //var metricInstance = _sourceInstanceCache.GetInstanceById(metricDesc.InstanceId);
                //_control.SystemWarning($"Пришел запоздавший показатель метрики {metricDesc.Name} от {metricInstance.Name}({metricInstance.SourceRef().Name})");

                var pop = _timeWindowBegin;

                for (; pop > value.Created + _metricInterval; pop -= _metricInterval) { }

                if (!_metricValues[metricDesc.Id].TryGetValue(pop, out var mergingMetricValue))
                    return;

                var locker = _metricValuesLockers.GetOrAdd(mergingMetricValue.Id, new object());

                lock (locker)
                {
                    if (mergingMetricValue.Created >= value.Created + _metricInterval) return;

                    switch (metricDesc.Type)
                    {
                        case MetricType.Accumulator:
                            mergingMetricValue.Value += value.Value;
                            mergingMetricValue.AggValuesCount++;
                            break;
                        case MetricType.Gauge:

                            mergingMetricValue.Value =
                                (mergingMetricValue.Value * mergingMetricValue.AggValuesCount + value.Value) /
                                (mergingMetricValue.AggValuesCount                            + 1);

                            mergingMetricValue.AggValuesCount++;
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    _repository.UpdateMetricValue(mergingMetricValue);
                }
            }
        }

        private void AggregateTask(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                //waiting at the beginning of each cycle because last stub was initialised with creating time as DateTime.Now
                Task.Delay(_timeWindowBegin.AddSeconds(_settings.MetricInterval) - DateTime.UtcNow, token).Wait(token);

                _timeWindowBegin = _timeWindowBegin.AddSeconds(_settings.MetricInterval);

                foreach (var metricAggregatingValue in _metricAggregatingValues)
                {
                    var aggLocker = _metricValuesLockers.GetOrAdd(metricAggregatingValue.Key, new object());

                    var circularBuffer = _metricValues[metricAggregatingValue.Key];

                    //NOT THREAD SAFE, because this is the only place to add/delete items to circularBuffer
                    //but possible multiple readings, so FIRST Add first pointer, THEN remove 
                    circularBuffer.First += _metricInterval;
                    circularBuffer.TryRemove(circularBuffer.First - _metricInterval, out var newValue);

                    var locker = _metricValuesLockers.GetOrAdd(newValue.Id, new object());

                    lock (aggLocker)
                    lock (locker)
                    {
                        newValue.AggValuesCount = metricAggregatingValue.Value.AggsCount;
                        newValue.Value          = metricAggregatingValue.Value.Value;
                        newValue.Created        = _timeWindowBegin;

                        metricAggregatingValue.Value.AggsCount = 0;
                        metricAggregatingValue.Value.Value     = 0;

                        switch (_metricDescriptions[newValue.MetricId].Type)
                        {
                            case MetricType.Gauge:
                                if (newValue.AggValuesCount != 0) newValue.Value = newValue.Value / newValue.AggValuesCount;
                                break;
                            case MetricType.Accumulator:
                                break;

                            default:
                                throw new NotImplementedException();
                        }

                        _repository.UpdateMetricValue(newValue);
                    }

                    circularBuffer.AddOrUpdate(newValue.Created, newValue, (time, value) => newValue);
                    circularBuffer.Last = newValue.Created;
                }
            }
        }

        public void OnStop()
        {
            _tokenSource.Cancel();
            _control.ApplicationVerbose($"{nameof(CacheMetrics)} stopped");
        }

        public List<MetricDescription> GetMetricDescriptions()
        {
            return _metricDescriptions.Values.ToList();
        }

        public List<MetricValue> GetAllCurrentMetricValues()
        {
            return _metricAggregatingValues.Select(v =>
                {
                    var locker = _metricValuesLockers.GetOrAdd(v.Key, new object());

                    lock (locker)
                    {
                        var metricValue = new MetricValue()
                        {
                            AggValuesCount = v.Value.AggsCount,
                            MetricId       = v.Key,
                            Created        = DateTime.UtcNow,
                            Value          = v.Value.Value
                        };

                        switch (_metricDescriptions[v.Key].Type)
                        {
                            case MetricType.Accumulator:
                                break;
                            case MetricType.Gauge:
                                if (metricValue.AggValuesCount != 0) metricValue.Value /= metricValue.AggValuesCount;
                                break;

                            default:
                                throw new NotImplementedException();
                        }

                        return metricValue;
                    }
                })
               .ToList();
        }

        public MetricValue GetCurrentMetricValue(long metricId)
        {
            var aggregatingValue = _metricAggregatingValues[metricId];
            var locker           = _metricValuesLockers.GetOrAdd(metricId, new object());

            lock (locker)
            {
                var metricValue = new MetricValue()
                {
                    AggValuesCount = aggregatingValue.AggsCount,
                    MetricId       = metricId,
                    Created        = DateTime.UtcNow,
                    Value          = aggregatingValue.Value
                };

                switch (_metricDescriptions[metricId].Type)
                {
                    case MetricType.Accumulator:
                        break;
                    case MetricType.Gauge:
                        if (metricValue.AggValuesCount != 0) metricValue.Value /= metricValue.AggValuesCount;
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return metricValue;
            }
        }

        public List<MetricValue> GetHistoryMetricValues(long metricId, int count, DateTime? last = null)
        {
            var rez = new List<MetricValue>(count);

            if (!_metricValues.ContainsKey(metricId))
                return rez;

            var lastWindowCreationTime = _timeWindowBegin;
            last = last ?? _timeWindowBegin;

            for (; lastWindowCreationTime > last; lastWindowCreationTime -= _metricInterval) { }

            var pop = lastWindowCreationTime;

            for (int i = 0; i < count && pop > _metricValues[metricId].First; i++, pop -= _metricInterval)
            {
                rez.Add(_metricValues[metricId][pop]);
            }

            rez.Reverse();
            return rez;
        }


        private class AggMetricVal
        {
            public long Value;
            public int  AggsCount;
        }

        private class CircularBuffer<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
        {
            public TKey First { get; set; }
            public TKey Last  { get; set; }

            public CircularBuffer(IEnumerable<TValue> collection, Func<TValue, TKey> keySelector) : base(collection.ToDictionary(keySelector))
            {
                First = Keys.Min();
                Last  = Keys.Max();
            }
        }
    }
}