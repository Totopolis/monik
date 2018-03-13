using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Monik.Client;

namespace Monik.Service
{
	public class CacheKeepAlive : ICacheKeepAlive
	{
		private readonly IRepository _repository;
		private readonly ISourceInstanceCache _cache;
		private readonly IClientControl _control;

		private readonly Dictionary<int, KeepAlive_> _status;

	    public long LastKeepAliveId { get; private set; }

	    public CacheKeepAlive(IRepository aRepository, ISourceInstanceCache aCache, IClientControl aControl)
		{
			_repository = aRepository;
			_cache = aCache;
			_control = aControl;

		    // fill from database
		    List<KeepAlive_> top = _repository.GetLastKeepAlive(1000);

		    LastKeepAliveId = top?.FirstOrDefault()?.ID ?? 0;

		    _status = top?.GroupBy(ka => ka.InstanceID)
		                  .ToDictionary(g => g.Key, g => g.OrderByDescending(ka => ka.Created).First()) ??
		              new Dictionary<int, KeepAlive_>();

            _control.ApplicationVerbose("CacheKeepAlive created");
		}

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
		    List<KeepAlive_> result;
		    lock (this)
		    {
		        result = _status.Values.ToList();
		    }

		    if (aFilter.Groups.Length == 0 && aFilter.Instances.Length == 0)
		    {
		        result.RemoveAll(ka => !_cache.IsDefaultInstance(ka.InstanceID));
		        return result;
		    }

		    var filteredRes = new List<KeepAlive_>();

		    foreach (var ka in result)
		    {
		        filteredRes.AddRange(aFilter.Groups.Where(gr => _cache.IsInstanceInGroup(ka.InstanceID, gr)).Select(gr => ka));

		        filteredRes.AddRange(aFilter.Instances.Where(inst => inst == ka.InstanceID).Select(inst => ka));
		    }

		    return filteredRes;
		}

	} //end of class
}
