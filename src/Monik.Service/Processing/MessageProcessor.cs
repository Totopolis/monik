using System;
using Monik.Common;

namespace Monik.Service
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly IMonikServiceSettings _settings;
        private readonly IRepository _repository;
        private readonly ICacheLog _cacheLog;
        private readonly ICacheKeepAlive _cacheKeepAlive;
        private readonly ICacheMetric _cacheMetric;
        private readonly IMonik _monik;

        public const string TotalMessages = "TotalMessages";
        public const string LogCount = "LogCount";
        public const string KeepAliveCount = "KeepAliveCount";
        public const string MeasureCount = "MeasureCount";

        public MessageProcessor(IMonikServiceSettings settings, IRepository repository, 
            ICacheLog cacheLog, ICacheKeepAlive cacheKeepAlive, ICacheMetric cacheMetric, 
            IMonik monik)
        {
            _settings = settings;
            _repository = repository;
            _cacheLog = cacheLog;
            _cacheKeepAlive = cacheKeepAlive;
            _cacheMetric = cacheMetric;
            _monik = monik;

            _cleaner = Scheduler.CreatePerHour(_monik, CleanerTask, "cleaner");
            _statist = Scheduler.CreatePerHour(_monik, StatistTask, "statist");
        }

        private readonly Scheduler _cleaner;
        private readonly Scheduler _statist;

        public void OnStart()
        {
            _cleaner.OnStart();
            _statist.OnStart();

            _monik.ApplicationVerbose("MessageProcessor started");
        }

        private void CleanerTask()
        {
            try
            {
                // cleanup logs
                var logDeep = _settings.DayDeepLog;
                var logThreshold = _repository.GetLogThreshold(logDeep);
                if (logThreshold.HasValue)
                {
                    var count = _repository.CleanUpLog(logThreshold.Value);
                    _monik.LogicInfo("Cleaner delete Log: {0} rows", count);
                }

                // cleanup keep-alive
                var kaDeep = _settings.DayDeepKeepAlive;
                var kaThreshold = _repository.GetKeepAliveThreshold(kaDeep);
                if (kaThreshold.HasValue)
                {
                    var count = _repository.CleanUpKeepAlive(kaThreshold.Value);
                    _monik.LogicInfo("Cleaner delete KeepAlive: {0} rows", count);
                }
            }
            catch (Exception ex)
            {
                _monik.ApplicationError("CleanerTask: {0}", ex.Message);
            }
        }

        private void StatistTask()
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                DateTime hs = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);

                _repository.CreateHourStat(hs, _cacheLog.LastId, _cacheKeepAlive.LastId);
            }
            catch (Exception ex)
            {
                _monik.ApplicationError("StatistTask: {0}", ex.Message);
            }
        }

        public void OnStop()
        {
            _statist.OnStop();
            _cleaner.OnStop();
        }

        public void Process(Event ev, Instance instance)
        {
            _monik.Measure(TotalMessages, AggregationType.Accumulator, 1);

            switch (ev.MsgCase)
            {
                case Event.MsgOneofCase.None:
                    throw new NotSupportedException("Bad event type");
                case Event.MsgOneofCase.Ka:
                    _monik.Measure(KeepAliveCount, AggregationType.Accumulator, 1);
                    var ka = CreateKeepAlive(ev, instance);
                    _cacheKeepAlive.Add(ka);
                    break;
                case Event.MsgOneofCase.Lg:
                    _monik.Measure(LogCount, AggregationType.Accumulator, 1);
                    var lg = CreateLog(ev, instance);
                    _cacheLog.Add(lg);
                    break;
                case Event.MsgOneofCase.Mc:
                    _monik.Measure(MeasureCount, AggregationType.Accumulator, 1);
                    _cacheMetric.OnNewMeasure(instance, ev);
                    break;
                default:
                    throw new NotSupportedException("Bad event type");
            }
        }

        public void FinalizeProcessing()
        {
            _cacheKeepAlive.Flush();
            _cacheLog.Flush();
        }

        private static KeepAlive_ CreateKeepAlive(Event eventKeepAlive, Instance instance)
        {
            return new KeepAlive_
            {
                Created = DateTimeOffset.FromUnixTimeMilliseconds(eventKeepAlive.Created).UtcDateTime,
                Received = DateTime.UtcNow,
                InstanceID = instance.ID
            };
        }

        private static Log_ CreateLog(Event eventLog, Instance instance)
        {
            return new Log_
            {
                Created = DateTimeOffset.FromUnixTimeMilliseconds(eventLog.Created).UtcDateTime,
                Received = DateTime.UtcNow,
                Level = (byte)eventLog.Lg.Level,
                Severity = (byte)eventLog.Lg.Severity,
                InstanceID = instance.ID,
                Format = (byte)eventLog.Lg.Format,
                Body = eventLog.Lg.Body,
                Tags = eventLog.Lg.Tags
            };
        }
    }//end of class
}
