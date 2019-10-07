using Monik.Common;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Monik.Service
{
    public abstract class CacheBase<TEntity> : ICacheBase<TEntity> where TEntity : ICacheEntity
    {
        protected readonly IRepository _repository;
        protected readonly IMonik _monik;
        protected ICacheSourceInstance _cache;

        private readonly TimingHelper _timing;
        private readonly ConcurrentQueue<TEntity> _pendingEntities;
        
        private long _lastId;
        public long LastId
        {
            get => Interlocked.Read(ref _lastId);
            protected set => Interlocked.Exchange(ref _lastId, value);
        }

        protected CacheBase(IRepository repository, ICacheSourceInstance cache, IMonik monik)
        {
            _repository = repository;
            _monik = monik;
            _cache = cache;

            _timing = TimingHelper.Create(_monik);
            _pendingEntities = new ConcurrentQueue<TEntity>();
        }

        public abstract void OnStart();
        public abstract void OnStop();

        public virtual void Flush()
        {
            _timing.Begin();

            var data = new List<TEntity>();
            while (_pendingEntities.TryDequeue(out var item))
                data.Add(item);

            WriteEntites(data);

            _timing.EndAndMeasure(WriteTimeMetric);
        }

        public virtual void Add(TEntity entity)
        {
            entity.ID = Interlocked.Increment(ref _lastId);
            _pendingEntities.Enqueue(entity);
        }

        public int PendingAmount => _pendingEntities.Count;

        protected abstract void WriteEntites(IEnumerable<TEntity> entities);
        protected abstract string WriteTimeMetric { get; }

    } //end of class
}
