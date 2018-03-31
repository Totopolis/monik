using System.Collections.Generic;
using System.Linq;
using Monik.Client;

namespace Monik.Service
{
    public class CacheKeepAlive : ICacheKeepAlive
    {
        private readonly IRepository _repository;
        private readonly ISourceInstanceCache _cache;
        private readonly IClientControl _control;

        private readonly Dictionary<int, KeepAlive_> _status;

        public CacheKeepAlive(IRepository aRepository, ISourceInstanceCache aCache, IClientControl aControl)
        {
            _repository = aRepository;
            _cache = aCache;
            _control = aControl;

            _status = new Dictionary<int, KeepAlive_>();

            _control.ApplicationVerbose("CacheKeepAlive created");
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

            _control.ApplicationVerbose("CacheKeepAlive started");
        }

        public void OnStop()
        {
            // nothing
        }

        public long LastKeepAliveId { get; private set; }

        public void OnNewKeepAlive(KeepAlive_ aKeepAlive)
        {
            lock (this)
            {
                LastKeepAliveId = aKeepAlive.ID;

                if (_status.ContainsKey(aKeepAlive.InstanceID))
                {
                    if (_status[aKeepAlive.InstanceID].Created < aKeepAlive.Created)
                        _status[aKeepAlive.InstanceID] = aKeepAlive;
                }
                else
                    _status.Add(aKeepAlive.InstanceID, aKeepAlive);
            } // TODO: optimize lock
        }

        public List<KeepAlive_> GetKeepAlive2(KeepAliveRequest aFilter)
        {
            lock (this)
            {
                List<KeepAlive_> result = _status.Values.ToList();

                if (aFilter.Groups.Length == 0 && aFilter.Instances.Length == 0)
                {
                    result.RemoveAll(ka => !_cache.IsDefaultInstance(ka.InstanceID));
                    return result;
                }
                else
                {
                    var filteredRes = new List<KeepAlive_>();

                    foreach (var ka in result)
                    {
                        foreach (var gr in aFilter.Groups)
                            if (_cache.IsInstanceInGroup(ka.InstanceID, gr))
                                filteredRes.Add(ka);

                        foreach (var inst in aFilter.Instances)
                            if (inst == ka.InstanceID)
                                filteredRes.Add(ka);
                    }

                    return filteredRes;
                }
            } // TODO: optimize lock
        }

    } //end of class
}
