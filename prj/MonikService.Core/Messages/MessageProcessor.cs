using System;
using Monik.Client;
using Monik.Common;
using MonikService.Core.Cache;
using MonikService.Core.Repository;
using MonikService.Core.Settings;

namespace MonikService.Core.Messages
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly IServiceSettings _settings;
        private readonly IRepository      _repository;
        private readonly ICacheLog        _cacheLog;
        private readonly ICacheKeepAlive  _cacheKeepAlive;
        private readonly IClientControl   _control;
        private readonly ICacheMetrics    _cacheMetrics;

        public MessageProcessor(IServiceSettings aSettings,
                                IRepository      aRepository,
                                ICacheLog        aCacheLog,
                                ICacheKeepAlive  aCacheKeepAlive,
                                IClientControl   aControl,
                                ICacheMetrics    aCacheMetrics)
        {
            _settings       = aSettings;
            _repository     = aRepository;
            _cacheLog       = aCacheLog;
            _cacheKeepAlive = aCacheKeepAlive;
            _cacheMetrics   = aCacheMetrics;
            _control        = aControl;

            _cleaner = Scheduler.CreatePerHour(_control, CleanerTask, "cleaner");
            _statist = Scheduler.CreatePerHour(_control, StatistTask, "statist");

            _control.ApplicationVerbose("MessageProcessor created");
        }

        private readonly Scheduler _cleaner;
        private readonly Scheduler _statist;

        public void OnStart()
        {
            _cleaner.OnStart();
            _statist.OnStart();

            _control.ApplicationVerbose("MessageProcessor started");
        }

        private void CleanerTask()
        {
            try
            {
                // cleanup logs
                var logDeep      = _settings.DayDeepLog;
                var logThreshold = _repository.GetLogThreshold(logDeep);

                if (logThreshold.HasValue)
                {
                    var count = _repository.CleanUpLog(logThreshold.Value);
                    _control.LogicInfo($"Cleaner delete Log: {count} rows");
                }

                // cleanup keep-alive
                var kaDeep      = _settings.DayDeepKeepAlive;
                var kaThreshold = _repository.GetKeepAliveThreshold(kaDeep);

                if (kaThreshold.HasValue)
                {
                    var count = _repository.CleanUpKeepAlive(kaThreshold.Value);
                    _control.LogicInfo($"Cleaner delete KeepAlive: {count} rows");
                }
            }
            catch (Exception ex)
            {
                _control.ApplicationError($"CleanerTask: {ex.Message}");
            }
        }

        private void StatistTask()
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                DateTime hs  = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);

                _repository.CreateHourStat(hs, _cacheLog.LastLogId, _cacheKeepAlive.LastKeepAliveId);
            }
            catch (Exception ex)
            {
                _control.ApplicationError($"StatistTask: {ex.Message}");
            }
        }

        public void OnStop()
        {
            _statist.OnStop();
            _cleaner.OnStop();
        }

        public void Process(Event aEvent, Instance aInstance)
        {
            switch (aEvent.MsgCase)
            {
                case Event.MsgOneofCase.None:
                    throw new NotSupportedException("Bad event type");
                case Event.MsgOneofCase.Ka:
                    WriteKeepAlive(aEvent, aInstance);
                    break;
                case Event.MsgOneofCase.Lg:
                    WriteLog(aEvent, aInstance);
                    break;
                case Event.MsgOneofCase.Metric:
                    WriteMetrics(aEvent, aInstance);
                    break;
                default:
                    throw new NotSupportedException("Bad event type");
            }
        }

        private void WriteMetrics(Event aEvent, Instance aInstance)
        {
            _cacheMetrics.AddMetricAggregatingValue(
                new MetricDescription()
                {
                    InstanceId = aInstance.ID,
                    Name       = aEvent.Metric.Name,
                    Type       = aEvent.Metric.MetricType
                },
                new MetricValue()
                {
                    Created = Helper.FromMillisecondsSinceUnixEpoch(aEvent.Created),
                    Value   = aEvent.Metric.Value
                });
        }

        private void WriteKeepAlive(Event aEventLog, Instance aInstance)
        {
            KeepAlive_ row = new KeepAlive_()
            {
                Created    = Helper.FromMillisecondsSinceUnixEpoch(aEventLog.Created),
                Received   = DateTime.UtcNow,
                InstanceID = aInstance.ID
            };

            _repository.CreateKeepAlive(row);

            _cacheKeepAlive.OnNewKeepAlive(row);
        }

        private void WriteLog(Event aEventLog, Instance aInstance)
        {
            Log_ row = new Log_()
            {
                Created    = Helper.FromMillisecondsSinceUnixEpoch(aEventLog.Created),
                Received   = DateTime.UtcNow,
                Level      = (byte) aEventLog.Lg.Level,
                Severity   = (byte) aEventLog.Lg.Severity,
                InstanceID = aInstance.ID,
                Format     = (byte) aEventLog.Lg.Format,
                Body       = aEventLog.Lg.Body,
                Tags       = aEventLog.Lg.Tags
            };

            _repository.CreateLog(row);

            _cacheLog.OnNewLog(row);
        }
    } //end of class
}