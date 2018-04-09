using System;
using System.Collections.Generic;
using System.Linq;
using Monik.Common;
using System.Collections.Concurrent;

namespace Monik.Service
{
    public class CacheMetric : ICacheMetric
    {
        private readonly IRepository _repository;
        private readonly IMonik _monik;

        private ISourceInstanceCache _cache;

        public CacheMetric(IRepository repository, ISourceInstanceCache cache, IMonik monik)
        {
            _repository = repository;
            _monik = monik;

            _cache = cache;

            _monik.ApplicationVerbose("CacheMetric created");
        }

        public void OnStart()
        {
            _monik.ApplicationVerbose("CacheMetric started");
        }

        public void OnStop()
        {
            // nothing
        }
    } //end of class
}
