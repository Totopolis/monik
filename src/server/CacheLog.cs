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
        private readonly IMonik _control;

        private ConcurrentQueue<Log_> _logs;
        private ISourceInstanceCache _cache;

        private long _oldestLogId;
        public long OldestLogId
        {
            get { lock (this) return _oldestLogId; }
            private set { lock (this) _oldestLogId = value; }
        }

        public CacheLog(IRepository aRepository, ISourceInstanceCache aCache, IMonik aControl)
        {
            _repository = aRepository;
            _control = aControl;

            _logs = null;
            _cache = aCache;
            OldestLogId = 0;

            _control.ApplicationVerbose("CacheLog created");
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

            _control.ApplicationVerbose("CacheLog started");
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

        public void OnNewLog(Log_ aLog)
        {
            _logs.Enqueue(aLog);
            LastLogId = aLog.ID;

            Log_ xx;
            while (_logs.Count > _logsDeep)
                if (_logs.TryDequeue(out xx) && xx.ID > OldestLogId) // TODO: remove second condition?
                    OldestLogId = xx.ID;
        }

        private bool IsFiltered5(Log_ aLog, LogRequest aFilter)
        {
            if (aFilter.SeverityCutoff.HasValue && aLog.Severity > aFilter.SeverityCutoff.Value)
                return false;

            if (aFilter.Level.HasValue && aLog.Level != aFilter.Level.Value)
                return false;

            bool groupsEmpty = aFilter.Groups == null || aFilter.Groups.Length == 0;
            bool instancesEmpty = aFilter.Instances == null || aFilter.Instances.Length == 0;

            if (groupsEmpty && instancesEmpty && _cache.IsDefaultInstance(aLog.InstanceID))
                return true;

            bool instanceIn = instancesEmpty
                ? false
                : aFilter.Instances.Contains(aLog.InstanceID);

            bool groupIn = groupsEmpty
                ? false
                : aFilter.Groups.Where(x => _cache.IsInstanceInGroup(aLog.InstanceID, x)).Count() > 0;

            return instanceIn | groupIn;
        }

        public List<Log_> GetLogs5(LogRequest aFilter)
        {
            List<Log_> result = new List<Log_>();

            if (aFilter == null)
                return result;

            if (aFilter.LastId.HasValue && aFilter.LastId.Value < OldestLogId)
                return result;

            result = aFilter.LastId.HasValue
                ? _logs.Where(lg => lg.ID > aFilter.LastId.Value).ToList()
                : _logs.ToList();

            // TODO: remove magic number
            int top = aFilter.Top ?? 10;

            if (!aFilter.LastId.HasValue)
            {
                result = result.FindAll(x => IsFiltered5(x, aFilter))
                    .OrderByDescending(x => x.ID)
                    .Take(top)
                    .OrderBy(x => x.ID)
                    .ToList();
            }
            else
                result = result
                    .FindAll(x => IsFiltered5(x, aFilter))
                    .Take(top)
                    .ToList();

            return result;
        }
    } //end of class
}
