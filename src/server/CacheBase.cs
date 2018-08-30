using System.Collections.Generic;
using Monik.Common;

namespace Monik.Service
{
    public abstract class CacheBase<TEntity> : ICacheBase<TEntity> where TEntity : ICacheEntity
    {
        protected readonly IRepository _repository;
        protected readonly IMonik _monik;
        protected ISourceInstanceCache _cache;

        private readonly TimingHelper _timing;
        private readonly List<TEntity> _pendingEntities;

        private long _lastId;
        public long LastId
        {
            get { lock (this) return _lastId; }
            protected set { lock (this) _lastId = value; }
        }

        public CacheBase(IRepository repository, ISourceInstanceCache cache, IMonik monik)
        {
            _repository = repository;
            _monik = monik;
            _cache = cache;

            _timing = TimingHelper.Create(_monik);
            _pendingEntities = new List<TEntity>();
        }

        public abstract void OnStart();
        public abstract void OnStop();

        public virtual void Flush()
        {
            _timing.Begin();

            WriteEntites(_pendingEntities);
            _pendingEntities.Clear();

            _timing.EndAndMeasure(WriteTimeMetric);
        }

        public virtual void Add(TEntity entity)
        {
            entity.ID = ++LastId;
            _pendingEntities.Add(entity);
        }

        protected abstract void WriteEntites(IEnumerable<TEntity> entities);
        protected abstract string WriteTimeMetric { get; }

    } //end of class
}
