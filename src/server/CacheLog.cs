using System;
using System.Collections.Generic;
using System.Linq;
using Monik.Client;

namespace Monik.Service
{
	public class CacheLog : ICacheLog
	{
		private readonly IRepository _repository;
		private readonly IClientControl _control;

		private List<Log_> _logs;
		private ISourceInstanceCache _cache;
		private long _oldestId;

		public CacheLog(IRepository aRepository, ISourceInstanceCache aCache, IClientControl aControl)
		{
			_repository = aRepository;
			_control = aControl;

			_logs = null;
			_cache = aCache;
			_oldestId = 0;

			_control.ApplicationVerbose("CacheLog created");
		}

		public void OnStart()
		{
			// load from database

			// 1. last IDs
			LastLogId = _repository.GetMaxLogId();

			// 2. load top logs
			_logs = _repository.GetLastLogs(1000);

			_oldestId = _logs.Count == 0 ? 0 : _logs.Min(lg => lg.ID);

			_control.ApplicationVerbose("CacheLog started");
		}

		public void OnStop()
		{
			// nothing
		}

		public long LastLogId { get; private set; }

		public void OnNewLog(Log_ aLog)
		{
			lock (this)
			{
				_logs.Add(aLog);
				LastLogId = aLog.ID;
			}

			// TODO: pop overhead logs
		}

		public List<Log_> GetLogs4(int? aGroup, long? aLastId, int? aSeverityCutoff, int? aLevel, int? aTop)
		{
			throw new NotImplementedException();
		}

		private bool IsFiltered(Log_ aLog, LogsFilter[] aFilters)
		{
			foreach (var it in aFilters)
			{
				bool result = true;

				if (it.FilterBySource)
				{
					Source src = _cache.GetSourceByInstanceId(aLog.InstanceID);
					if (src == null)
						return false;

					result &= src.ID == it.SourceId;
				}

				if (it.FilterByInstance)
					result &= it.InstanceId == aLog.ID;

				if (it.FilterByLevel)
					result &= it.Level == aLog.Level;

				if (it.FilterBySeverity)
					result &= it.Severity == aLog.Severity;

				if (it.EnableCutoffSeverity)
					result &= it.CutoffSeverity >= aLog.Severity;

				if (result)
					return true;
			} //foreach

			return false;
		}

		public List<Log_> GetLogs(int? aTop, Order aOrder, long? aLastID, LogsFilter[] aFilters)
		{
			lock (this)
			{
				List<Log_> result = new List<Log_>();

				if (aLastID.HasValue)
					if (aLastID.Value < _oldestId)
						return result;
					else
					{
						lock (this)
							result = _logs.FindAll(lg => lg.ID > aLastID.Value).ToList();

						result = (aOrder == Order.Desc ? result.OrderByDescending(lg => lg.ID) : result.OrderBy(lg => lg.ID)).ToList();
					}
				else
					lock (this)
						result = (aOrder == Order.Desc ? _logs.OrderByDescending(lg => lg.ID) : _logs.OrderBy(lg => lg.ID)).ToList();

				// TODO: stop when top reach
				if (aFilters != null && aFilters.Length > 0)
					result = result.FindAll(lg => IsFiltered(lg, aFilters)).ToList();

				// remove not default groups
				result.RemoveAll(lg => !_cache.IsDefaultInstance(lg.InstanceID));

				if (aTop.HasValue)
					result = result.Take(aTop.Value).ToList();

				return result;
			} // TODO: optimize
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

			if (aFilter.LastId.HasValue && aFilter.LastId.Value < _oldestId)
				return result;

			lock (this)
			{
				result = aFilter.LastId.HasValue
					? _logs.FindAll(lg => lg.ID > aFilter.LastId.Value).ToList()
					: _logs.Select(x => x).ToList();
			}

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
