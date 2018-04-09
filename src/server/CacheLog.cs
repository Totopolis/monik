using System;
using System.Collections.Generic;
using System.Linq;
using Monik.Common;
using System.Collections.Concurrent;

namespace Monik.Service
{
    public class CacheLog : ICacheLog
    {
        private readonly IRepository _repository;
        private readonly IMonik _monik;

        private ConcurrentQueue<Log_> _logs;
        private ISourceInstanceCache _cache;

        private long _oldestLogId;
        public long OldestLogId
        {
            get { lock (this) return _oldestLogId; }
            private set { lock (this) _oldestLogId = value; }
        }

        public CacheLog(IRepository repository, ISourceInstanceCache cache, IMonik monik)
        {
            _repository = repository;
            _monik = monik;

            _logs = null;
            _cache = cache;
            OldestLogId = 0;

            _monik.ApplicationVerbose("CacheLog created");
        }

        // TODO: use database parameter
        private readonly int _logsDeep = 20000;

        public void OnStart()
        {
            // load from database

            // 1. last IDs
            LastLogId = _repository.GetMaxLogId();

            // 2. load top logs
            var lastLogsFromDB = _repository.GetLastLogs(_logsDeep);
            _logs = new ConcurrentQueue<Log_>(lastLogsFromDB);

            OldestLogId = _logs.Count == 0 ? 0 : _logs.Min(lg => lg.ID);

            _monik.ApplicationVerbose("CacheLog started");
        }

        public void OnStop()
        {
            // nothing
        }

        private long _lastLogId;
        public long LastLogId
        {
            get { lock (this) return _lastLogId; }
            private set { lock (this) _lastLogId = value; }
        }

        public void OnNewLog(Log_ log)
        {
            _logs.Enqueue(log);
            LastLogId = log.ID;

            while (_logs.Count > _logsDeep)
                if (_logs.TryDequeue(out Log_ xx) && xx.ID > OldestLogId) // TODO: remove second condition?
                    OldestLogId = xx.ID;
        }

        private bool IsFiltered5(Log_ log, LogRequest filter)
        {
            if (filter.SeverityCutoff.HasValue && log.Severity > filter.SeverityCutoff.Value)
                return false;

            if (filter.Level.HasValue && log.Level != filter.Level.Value)
                return false;

            bool groupsEmpty = filter.Groups == null || filter.Groups.Length == 0;
            bool instancesEmpty = filter.Instances == null || filter.Instances.Length == 0;

            if (groupsEmpty && instancesEmpty && _cache.IsDefaultInstance(log.InstanceID))
                return true;

            bool instanceIn = instancesEmpty
                ? false
                : filter.Instances.Contains(log.InstanceID);

            bool groupIn = groupsEmpty
                ? false
                : filter.Groups.Where(x => _cache.IsInstanceInGroup(log.InstanceID, x)).Count() > 0;

            return instanceIn | groupIn;
        }

        public List<Log_> GetLogs5(LogRequest filter)
        {
            List<Log_> result = new List<Log_>();

            if (filter == null)
                return result;

            if (filter.LastId.HasValue && filter.LastId.Value < OldestLogId)
                return result;

            result = filter.LastId.HasValue
                ? _logs.Where(lg => lg.ID > filter.LastId.Value).ToList()
                : _logs.ToList();

            // TODO: remove magic number
            int top = filter.Top ?? 10;

            if (!filter.LastId.HasValue)
            {
                result = result.FindAll(x => IsFiltered5(x, filter))
                    .OrderByDescending(x => x.ID)
                    .Take(top)
                    .OrderBy(x => x.ID)
                    .ToList();
            }
            else
                result = result
                    .FindAll(x => IsFiltered5(x, filter))
                    .Take(top)
                    .ToList();

            return result;
        }
    } //end of class
}
