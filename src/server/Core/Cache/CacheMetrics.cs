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
        
        private readonly ConcurrentDictionary<long, MetricDescription> _metricDescriptions;
        private readonly ConcurrentDictionary<ValueTuple<int, string>, MetricDescription> _metricDescriptionsMap;
        private readonly ConcurrentDictionary<long, ConcurrentMaxSizedQueue<MetricValue>> _metricValues;
        private readonly ConcurrentDictionary<long, ConcurrentBag<long>> _metricAggregatingValues;
        
        private readonly int _stubsCount;
        private readonly ConcurrentDictionary<long, object> _metricValuesLockers = new ConcurrentDictionary<long, object>();

        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private Task _metricsAggregator;

        public CacheMetrics(IRepository aRepository, ISourceInstanceCache aCache, IClientControl aControl, IServiceSettings settings)
        {
            _repository = aRepository;
            _sourceInstanceCache = aCache;
            _control = aControl;
            _settings = settings;


            // fill from database
            var _descriptions = _repository.GetMetricDescriptions();
            
            _metricDescriptionsMap = new ConcurrentDictionary<(int, string), MetricDescription>(_descriptions.ToDictionary(d => new ValueTuple<int, string>(d.InstanceId, d.Name)));

            _metricDescriptions = new ConcurrentDictionary<long, MetricDescription>(_descriptions.ToDictionary(d => d.Id));

            _stubsCount = _settings.MetricDeepMinutes * 60 / _settings.MetricInterval;

            var values = _repository
                .GetAllMetricValues()
                .GroupBy(mv => mv.MetricId)
                .ToDictionary(g => g.Key, g => g.OrderBy(mv => mv.Created).ToList());

            var metricInterval = TimeSpan.FromSeconds(_settings.MetricInterval);

            //Update Stubs in DB
            foreach (var mValues in values)
            {
                var tmp = _stubsCount - mValues.Value.Count;
                if (tmp > 0)
                {
                    var stabsToAdd = new List<MetricValue>(tmp);
                    var popTime = DateTime.Now;
                    int i;
                    for (i = 0; i < tmp && popTime > mValues.Value.Last().Created; i++ , popTime-= metricInterval)
                        stabsToAdd.Add(new MetricValue()
                        {
                            AggValuesCount = 0,
                            Created = popTime,
                            MetricId = mValues.Key,
                            Value = 0
                        });

                    popTime = mValues.Value.First().Created - metricInterval;
                    for (; i < tmp; i++, popTime -= metricInterval)
                    {
                        stabsToAdd.Add(new MetricValue()
                        {
                            AggValuesCount = 0,
                            Created = popTime,
                            MetricId = mValues.Key,
                            Value = 0
                        });
                    }
                    
                    _repository.AddMetricValueStubs(stabsToAdd);
                    mValues.Value.AddRange(stabsToAdd);
                }
                if (tmp < 0)
                {
                    _repository.DeleteMetricValueStubs(-tmp, mValues.Key);
                    mValues.Value.RemoveRange(0, tmp);
                }
            }

            _metricValues = new ConcurrentDictionary<long, ConcurrentMaxSizedQueue<MetricValue>>(values.Select(v =>
                new KeyValuePair<long, ConcurrentMaxSizedQueue<MetricValue>>(v.Key,
                    new ConcurrentMaxSizedQueue<MetricValue>(v.Value.OrderBy(mv => mv.Created)))));

            _metricAggregatingValues = new ConcurrentDictionary<long, ConcurrentBag<long>>(_metricDescriptions.Keys.ToDictionary(md => md, _ => new ConcurrentBag<long>()));

            _control.ApplicationVerbose($"{nameof(CacheMetrics)} created");

            _metricsAggregator = Task.Run(() => AggregateTask(_tokenSource.Token), _tokenSource.Token);
        }

        private void AggregateTask(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                //waiting at the beginning of each cycle because last stub was initialised with creating time as DateTime.Now
                Task.Delay(_settings.MetricInterval * 1000, token).Wait(token);

                foreach (var metricAggregatingValue in _metricAggregatingValues)
                {
                    _metricValues[metricAggregatingValue.Key].TryDequeue(out var newValue);
                    var locker = _metricValuesLockers.GetOrAdd(newValue.Id, new object());
                    lock (locker)
                    {
                        newValue.AggValuesCount = 0;
                        newValue.Value = 0;
                        newValue.Created = DateTime.Now;

                        while (metricAggregatingValue.Value.TryTake(out var aggValue))
                        {
                            newValue.AggValuesCount++;
                            newValue.Value += aggValue;
                        }

                        switch (_metricDescriptions[newValue.MetricId].Type)
                        {
                            case MetricType.Gauge:
                                if (newValue.AggValuesCount != 0)
                                    newValue.Value = newValue.Value / newValue.AggValuesCount;
                                break;
                            case MetricType.Accumulator:
                                break;

                            default:
                                throw new NotImplementedException();
                        }

                        _repository.UpdateMetricValue(newValue);
                    }
                    _metricValues[metricAggregatingValue.Key].Enqueue(newValue);
                }
            }
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
                        var metricInterval = TimeSpan.FromSeconds(_settings.MetricInterval);
                        var stabsToAdd = new List<MetricValue>(_stubsCount);
                        var popTime = DateTime.Now - metricInterval;

                        for (var i = _stubsCount - 1; i >= 0; i--, popTime -= metricInterval)
                            stabsToAdd.Add(new MetricValue()
                            {
                                AggValuesCount = 0,
                                Created = popTime,
                                MetricId = metricDesc.Id,
                                Value = 0
                            });

                        _repository.AddMetricValueStubs(stabsToAdd);
                        _metricValues.TryAdd(metricDesc.Id, new ConcurrentMaxSizedQueue<MetricValue>(stabsToAdd));
                        _metricAggregatingValues.TryAdd(metricDesc.Id, new ConcurrentBag<long>());
                    }
                }
            }

            if(value.Created.ToLocalTime() > _metricValues[metricDesc.Id].Last.Created)
                _metricAggregatingValues[metricDesc.Id].Add(value.Value);
            else
            {
                var metricInstance = _sourceInstanceCache.GetInstanceById(metricDesc.InstanceId);
                _control.SystemWarning($"Пришел запоздавший показатель метрики {metricDesc.Name} от {metricInstance.Name}({metricInstance.SourceRef().Name})");

                var mergingMetricValue = _metricValues[metricDesc.Id].FirstOrDefault(mv => mv.Created < value.Created);
                if (mergingMetricValue == null) return;

                var locker = _metricValuesLockers.GetOrAdd(mergingMetricValue.Id, new object());

                lock (locker)
                {
                    if (mergingMetricValue.Created >= value.Created) return;

                    switch (metricDesc.Type)
                    {
                        case MetricType.Accumulator:
                            mergingMetricValue.Value+= value.Value;
                            mergingMetricValue.AggValuesCount++;
                            break;
                        case MetricType.Gauge:
                            mergingMetricValue.Value =
                                (mergingMetricValue.Value * mergingMetricValue.AggValuesCount + value.Value) /
                                (mergingMetricValue.AggValuesCount + 1);
                            mergingMetricValue.AggValuesCount++;
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    _repository.UpdateMetricValue(mergingMetricValue);
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
            return _metricValues.Values.Select(v => v.Last).ToList();
        }
    }

    public class ConcurrentMaxSizedQueue<T> : ConcurrentQueue<T>
    {
        public int MaxSize { get; }
        
        public ConcurrentMaxSizedQueue(IEnumerable<T> collection) :base(collection)
        {
            MaxSize = Count;
            Last = collection.Last();
        }

        public T Last { get; private set; }

        private readonly object syncObject = new object();

        public new void Enqueue(T obj)
        {
            Last = obj;
            base.Enqueue(obj);
            lock (syncObject)
            {
                while (base.Count > MaxSize)
                {
                    T outObj;
                    base.TryDequeue(out outObj);
                }
            }
        }
    }
}