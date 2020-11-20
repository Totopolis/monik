using System.Collections.Generic;
using System.Linq;
using Monik.Common;

namespace Monik.Service
{
    public class CacheKeepAlive : CacheBase<KeepAlive_>, ICacheKeepAlive
    {
        private readonly Dictionary<int, KeepAlive_> _status;

        public CacheKeepAlive(IRepository repository, ICacheSourceInstance cache, IMonik monik)
            : base(repository, cache, monik)
        {
            _cache = cache;
            
            _status = new Dictionary<int, KeepAlive_>();
        }

        public override void OnStart()
        {
            // fill from database
            LastId = _repository.GetMaxKeepAliveId();

            List<KeepAlive_> top = _repository.GetLastKeepAlive(1000);
            top.Reverse();

            // fill current status
            foreach (var ka in top)
                if (!_status.ContainsKey(ka.InstanceID))
                    _status.Add(ka.InstanceID, ka);
                else if (_status[ka.InstanceID].Created < ka.Created)
                    _status[ka.InstanceID] = ka;

            _monik.ApplicationVerbose("CacheKeepAlive started");
        }

        public override void OnStop()
        {
            // nothing
        }

        public List<KeepAlive_> GetKeepAlive2(KeepAliveRequest filter)
        {
            lock (this)
            {
                List<KeepAlive_> result = _status.Values.ToList();

                if (filter.Groups.Length == 0 && filter.Instances.Length == 0)
                {
                    result.RemoveAll(ka => !_cache.IsDefaultInstance(ka.InstanceID));
                    return result;
                }
                else
                {
                    var filteredRes = new List<KeepAlive_>();

                    foreach (var ka in result)
                    {
                        foreach (var gr in filter.Groups)
                            if (_cache.IsInstanceInGroup(ka.InstanceID, gr))
                                filteredRes.Add(ka);

                        foreach (var inst in filter.Instances)
                            if (inst == ka.InstanceID)
                                filteredRes.Add(ka);
                    }

                    return filteredRes;
                }
            } // TODO: optimize lock
        }

        public override void Add(KeepAlive_ entity)
        {
            base.Add(entity);

            lock (this)
            {
                if (_status.ContainsKey(entity.InstanceID))
                {
                    if (_status[entity.InstanceID].Created < entity.Created)
                        _status[entity.InstanceID] = entity;
                }
                else
                    _status.Add(entity.InstanceID, entity);
            } // TODO: optimize lock
        }

        protected override void WriteEntites(IEnumerable<KeepAlive_> entities)
        {
            _repository.WriteKeepAlives(entities);
        }

        protected override string WriteTimeMetric => "WriteKeepAliveTime";
    } //end of class
}
