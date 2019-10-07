using Monik.Common;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Monik.Service
{
    public class CacheLog : CacheBase<Log_>, ICacheLog
    {
        private ConcurrentQueue<Log_> _logs;

        private long _oldestLogId;
        public long OldestLogId
        {
            get { lock (this) return _oldestLogId; }
            private set { lock (this) _oldestLogId = value; }
        }

        public CacheLog(IRepository repository, ICacheSourceInstance cache, IMonik monik)
            : base(repository, cache, monik)
        {
            _logs = new ConcurrentQueue<Log_>();
            OldestLogId = 0;

            _monik.ApplicationVerbose("CacheLog created");
        }

        // TODO: use database parameter
        private readonly int _logsDeep = 20000;

        public override void OnStart()
        {
            // load from database
            // 1. last IDs
            LastId = _repository.GetMaxLogId();
            // 2. load top logs
            var lastLogsFromDB = _repository.GetLastLogs(_logsDeep);
            _logs = new ConcurrentQueue<Log_>(lastLogsFromDB);

            OldestLogId = _logs.Count == 0 ? 0 : _logs.Min(lg => lg.ID);

            _monik.ApplicationVerbose("CacheLog started");
        }

        public override void OnStop()
        {
            // nothing
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

        public override void Add(Log_ entity)
        {
            base.Add(entity);

            _logs.Enqueue(entity);

            while (_logs.Count > _logsDeep)
                if (_logs.TryDequeue(out Log_ xx) && xx.ID > OldestLogId) // TODO: remove second condition?
                    OldestLogId = xx.ID;
        }

        protected override void WriteEntites(IEnumerable<Log_> entities)
        {
            _repository.WriteLogs(entities);
        }

        protected override string WriteTimeMetric => "WriteLogTime";
    } //end of class
}
