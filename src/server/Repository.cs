using Gerakul.FastSql;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;

namespace Monik.Service
{
	public class Repository : IRepository
	{
		private readonly IServiceSettings _settings;

		public Repository(IServiceSettings aSettings)
		{
			_settings = aSettings;
		}

        #region Sources

        public List<Source> GetAllSources()
		{
			return SimpleCommand.ExecuteQuery<Source>(_settings.DbConnectionString, "select * from [mon].[Source]").ToList();
		}

		public List<Instance> GetAllInstances()
		{
			return SimpleCommand.ExecuteQuery<Instance>(_settings.DbConnectionString, "select * from [mon].[Instance]").ToList();
		}

		public List<Group> GetAllGroupsAndFill()
		{
			List<Group> result =
				SimpleCommand.ExecuteQuery<Group>(_settings.DbConnectionString, "select * from [mon].[Group]").ToList();

			Dictionary<short, Group> dic = new Dictionary<short, Group>();
			foreach (var it in result)
				dic.Add(it.ID, it);

			var proto = new {GroupID = default(short), InstanceID = default(int)};
			var grInstances = SimpleCommand.ExecuteQueryAnonymous(proto, _settings.DbConnectionString,
				"select GroupID, InstanceID from [mon].[GroupInstance]");

			foreach (var it in grInstances)
				if (dic.ContainsKey(it.GroupID))
					dic[it.GroupID].Instances.Add(it.InstanceID);

			return result;
		}

		public void CreateNewSource(Source aSrc)
		{
			aSrc.ID = (short) MappedCommand.InsertAndGetId<Source>(_settings.DbConnectionString, "[mon].[Source]", aSrc, "ID");
		}

		public void CreateNewInstance(Instance aIns)
		{
			aIns.ID = (int) MappedCommand.InsertAndGetId<Instance>(_settings.DbConnectionString, "[mon].[Instance]", aIns, "ID");
		}

		public void AddInstanceToGroup(Instance aIns, Group aGroup)
		{
			var proto = new { GroupID = aGroup.ID, InstanceID = aIns.ID };
			MappedCommand.Insert(_settings.DbConnectionString, "mon.GroupInstance", proto, "ID");
		}

        #endregion

	    #region Logs

	    public long GetMaxLogId()
		{
			var result = SimpleCommand.ExecuteScalar(_settings.DbConnectionString, "select max(ID) from [mon].[Log]");
			return result == System.DBNull.Value ? 0 : (long) result;
		}

		public List<Log_> GetLastLogs(int aTop)
		{
			return
				SimpleCommand.ExecuteQuery<Log_>(_settings.DbConnectionString,
					$"select top {aTop} * from [mon].[Log] order by ID desc").OrderBy(x => x.ID).ToList();
		}

        public long? GetLogThreshold(int aDayDeep)
		{
			var logThreshold = SimpleCommand.ExecuteScalar(_settings.DbConnectionString,
				"select max(LastLogID) from mon.HourStat where Hour < DATEADD(DAY, -@p0, GETDATE())", aDayDeep);
			return logThreshold == System.DBNull.Value ? null : (long?)logThreshold;
		}

		public int CleanUpLog(long aLastLog)
		{
			return SimpleCommand.ExecuteNonQuery(_settings.DbConnectionString, "delete from mon.Log where ID < @p0", aLastLog);
		}

		public void CreateLog(Log_ aLog)
		{
			aLog.ID = (long) MappedCommand.InsertAndGetId<Log_>(_settings.DbConnectionString, "[mon].[Log]", aLog, "ID");
		}

        #endregion
        
	    #region KeepAlive

        public long GetMaxKeepAliveId()
		{
			var result = SimpleCommand.ExecuteScalar(_settings.DbConnectionString, "select max(ID) from [mon].[KeepAlive]");
			return result == System.DBNull.Value ? 0 : (long)result;
		}

		public List<KeepAlive_> GetLastKeepAlive(int aTop)
		{
			return
				SimpleCommand.ExecuteQuery<KeepAlive_>(_settings.DbConnectionString,
					$"select top {aTop} * from [mon].[KeepAlive] order by ID desc").ToList();
	    }

		public long? GetKeepAliveThreshold(int aDayDeep)
		{
			var kaThreshold = SimpleCommand.ExecuteScalar(_settings.DbConnectionString,
				"select max(LastKeepAliveID) from mon.HourStat where Hour < DATEADD(DAY, -@p0, GETDATE())", aDayDeep);
			return kaThreshold == System.DBNull.Value ? null : (long?)kaThreshold;
		}
        
		public int CleanUpKeepAlive(long aLastKeepAlive)
		{
			return SimpleCommand.ExecuteNonQuery(_settings.DbConnectionString, "delete from mon.KeepAlive where ID < @p0",
				aLastKeepAlive);
		}

		public void CreateKeepAlive(KeepAlive_ aKeepAlive)
		{
			aKeepAlive.ID =
				(long) MappedCommand.InsertAndGetId<KeepAlive_>(_settings.DbConnectionString, "[mon].[KeepAlive]", aKeepAlive, "ID");
		}

	    #endregion
        
		public void CreateHourStat(DateTime aHour, long aLastLogId, long aLastKeepAliveId)
		{
			var stat = new {Hour = aHour, LastLogID = aLastLogId, LastKeepAliveID = aLastKeepAliveId};
			MappedCommand.Insert(_settings.DbConnectionString, "[mon].[HourStat]", stat);
		}

		public List<EventQueue> GetEventSources()
		{
			return
				SimpleCommand.ExecuteQuery<EventQueue>(_settings.DbConnectionString, "select * from [mon].[EventQueue]").ToList();
		}
        
	    #region Metric

        public List<MetricDescription> GetMetricDescriptions()
        {
            return SimpleCommand.ExecuteQuery<MetricDescription>(_settings.DbConnectionString, $"select * from [mon].[{nameof(MetricDescription)}]").ToList();
	    }

	    public void AddMetricValueStubs(List<MetricValue> stabsToAdd)
	    {
	        foreach (var metricValue in stabsToAdd)
	            metricValue.Id = (int) MappedCommand.InsertAndGetId(_settings.DbConnectionString, "[mon].[MetricValue]",
	                metricValue, "Id");
	    }

	    public int DeleteMetricValueStubs(int stubsCount, long metricId)
	    {
	        var stubs = SimpleCommand.ExecuteQuery<MetricValue>(_settings.DbConnectionString,
	            $"select top {stubsCount} * from [mon].{nameof(MetricValue)} order by Id");

	        return SimpleCommand.ExecuteNonQuery(_settings.DbConnectionString,
	            $"delete from mon.{nameof(MetricValue)} where Created < {stubs.Max(s => s.Created)}");

	    }

	    public List<MetricValue> GetAllMetricValues()
	    {
	        return SimpleCommand
	            .ExecuteQuery<MetricValue>(_settings.DbConnectionString, $"select * from [mon].{nameof(MetricValue)}")
	            .ToList();
	    }

	    public List<MetricValue> GetMetricValues(int metricId)
	    {
	        return SimpleCommand
	            .ExecuteQuery<MetricValue>(_settings.DbConnectionString, $"select * from [mon].{nameof(MetricValue)} where MetricId = {metricId}")
	            .ToList();
        }

	    public void CreateMetricDescription(MetricDescription metricDescription)
	    {
	        metricDescription.Id = (int)MappedCommand.InsertAndGetId<MetricDescription>(_settings.DbConnectionString, $"[mon].[{nameof(MetricDescription)}]", metricDescription, "Id");
        }

	    public void UpdateMetricValue(MetricValue metricValue)
	    {
	        MappedCommand.Update(_settings.DbConnectionString, $"[mon].[{nameof(MetricValue)}]", metricValue, "Id");
	    }

	    #endregion
    } //end of class
}
