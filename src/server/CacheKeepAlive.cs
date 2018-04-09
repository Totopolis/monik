using System.Collections.Generic;
using System.Linq;
using Monik.Common;

namespace Monik.Service
{
    public class CacheKeepAlive : ICacheKeepAlive
    {
        private readonly IRepository _repository;
        private readonly ISourceInstanceCache _cache;
        private readonly IMonik _monik;

        private readonly Dictionary<int, KeepAlive_> _status;

        public CacheKeepAlive(IRepository repository, ISourceInstanceCache cache, IMonik monik)
        {
            _repository = repository;
            _cache = cache;
            _monik = monik;

            _status = new Dictionary<int, KeepAlive_>();

            _monik.ApplicationVerbose("CacheKeepAlive created");
        }

        public void OnStart()
        {
            // fill from database
            LastKeepAliveId = _repository.GetMaxKeepAliveId();

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

        public void OnStop()
        {
            // nothing
        }

        public long LastKeepAliveId { get; private set; }

        public void OnNewKeepAlive(KeepAlive_ keepAlive)
        {
            lock (this)
            {
                LastKeepAliveId = keepAlive.ID;

                if (_status.ContainsKey(keepAlive.InstanceID))
                {
                    if (_status[keepAlive.InstanceID].Created < keepAlive.Created)
                        _status[keepAlive.InstanceID] = keepAlive;
                }
                else
                    _status.Add(keepAlive.InstanceID, keepAlive);
            } // TODO: optimize lock
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

    } //end of class
}
