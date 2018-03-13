using System;
using Monik.Common;
using Monik.Client;

namespace Monik.Service
{
	public class MessageProcessor : IMessageProcessor
	{
		private readonly IServiceSettings _settings;
		private readonly IRepository _repository;
		private readonly ICacheLog _cacheLog;
		private readonly ICacheKeepAlive _cacheKeepAlive;
		private readonly IClientControl _control;

		public MessageProcessor(IServiceSettings aSettings, IRepository aRepository, ICacheLog aCacheLog,
			ICacheKeepAlive aCacheKeepAlive, IClientControl aControl)
		{
			_settings = aSettings;
			_repository = aRepository;
			_cacheLog = aCacheLog;
			_cacheKeepAlive = aCacheKeepAlive;
			_control = aControl;

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
				var logDeep = _settings.DayDeepLog;
				var logThreshold = _repository.GetLogThreshold(logDeep);
				if (logThreshold.HasValue)
				{
					var count = _repository.CleanUpLog(logThreshold.Value);
					_control.LogicInfo("Cleaner delete Log: {0} rows", count);
				}

				// cleanup keep-alive
				var kaDeep = _settings.DayDeepKeepAlive;
				var kaThreshold = _repository.GetKeepAliveThreshold(kaDeep);
				if (kaThreshold.HasValue)
				{
					var count = _repository.CleanUpKeepAlive(kaThreshold.Value);
					_control.LogicInfo("Cleaner delete KeepAlive: {0} rows", count);
				}
			}
			catch (Exception ex)
			{
				_control.ApplicationError("CleanerTask: {0}", ex.Message);
			}
		}

		private void StatistTask()
		{
			try
			{
				DateTime now = DateTime.UtcNow;
				DateTime hs = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);

				_repository.CreateHourStat(hs, _cacheLog.LastLogId, _cacheKeepAlive.LastKeepAliveId);
			}
			catch (Exception ex)
			{
				_control.ApplicationError("StatistTask: {0}", ex.Message);
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
					var ka = WriteKeepAlive(aEvent, aInstance);
					_cacheKeepAlive.OnNewKeepAlive(ka);
					break;
				case Event.MsgOneofCase.Lg:
					var lg = WriteLog(aEvent, aInstance);
					_cacheLog.OnNewLog(lg);
					break;
                case Event.MsgOneofCase.Metric:
                    
				default:
					throw new NotSupportedException("Bad event type");
			}
		}

		private KeepAlive_ WriteKeepAlive(Event aEventLog, Instance aInstance)
		{
			KeepAlive_ row = new KeepAlive_()
			{
				Created = Helper.FromMillisecondsSinceUnixEpoch(aEventLog.Created),
				Received = DateTime.UtcNow,
				InstanceID = aInstance.ID
			};

			_repository.CreateKeepAlive(row);

			return row;
		}

		private Log_ WriteLog(Event aEventLog, Instance aInstance)
		{
			Log_ row = new Log_()
			{
				Created = Helper.FromMillisecondsSinceUnixEpoch(aEventLog.Created),
				Received = DateTime.UtcNow,
				Level = (byte) aEventLog.Lg.Level,
				Severity = (byte) aEventLog.Lg.Severity,
				InstanceID = aInstance.ID,
				Format = (byte) aEventLog.Lg.Format,
				Body = aEventLog.Lg.Body,
				Tags = aEventLog.Lg.Tags
			};

			_repository.CreateLog(row);

			return row;
		}
	}//end of class
}
