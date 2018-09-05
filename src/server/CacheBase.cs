using Monik.Common;
using System.Collections.Generic;

namespace Monik.Service
{
    public abstract class CacheBase<TEntity> : ICacheBase<TEntity> where TEntity : ICacheEntity
    {
        protected readonly IRepository _repository;
        protected readonly IMonik _monik;
        protected ISourceInstanceCache _cache;

        private readonly TimingHelper _timing;
        private readonly List<TEntity> _pendingEntities;

        private readonly object _lockLastId = new object();
        private readonly object _lockPending = new object();

        private long _lastId;
        public long LastId
        {
            get
            {
                lock (_lockLastId)
                    return _lastId;
            }
            protected set
            {
                lock (_lockLastId)
                    _lastId = value;
            }
        }

        protected CacheBase(IRepository repository, ISourceInstanceCache cache, IMonik monik)
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
            lock (_lockPending)
            {
                _timing.Begin();

                WriteEntites(_pendingEntities);
                _pendingEntities.Clear();

                _timing.EndAndMeasure(WriteTimeMetric);
            }
        }

        public virtual void Add(TEntity entity)
        {
            lock (_lockLastId)
                entity.ID = ++LastId;
            lock (_lockPending)
                _pendingEntities.Add(entity);
        }

        protected abstract void WriteEntites(IEnumerable<TEntity> entities);
        protected abstract string WriteTimeMetric { get; }

    } //end of class
}
