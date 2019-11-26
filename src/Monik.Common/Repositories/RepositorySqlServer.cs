using System;
using System.Collections.Generic;
using System.Linq;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using Monik.Common;

namespace Monik.Service
{
    public class RepositorySqlServer : IRepository
    {
        private readonly IMonikServiceSettings _settings;
        private readonly DbContext _context;
        private readonly BulkOptions _bulkOptions;

        public RepositorySqlServer(IMonikServiceSettings settings, ContextProvider provider)
        {
            _settings = settings;
            _context = provider.CreateContext(_settings.DbConnectionString);

            _bulkOptions = provider.BulkOptions;
            if (_bulkOptions is SqlBulkOptions options)
            {
                options.BatchSize = _settings.WriteBatchSize;
                options.BulkCopyTimeout = _settings.WriteBatchTimeout;
            }
        }

        public List<Source> GetAllSources()
        {
            return _context
                .CreateSimple("select * from [mon].[Source] with(nolock)")
                .ExecuteQuery<Source>()
                .ToList();
        }

        public List<Instance> GetAllInstances()
        {
            return _context
                .CreateSimple("select * from [mon].[Instance] with(nolock)")
                .ExecuteQuery<Instance>()
                .ToList();
        }

        public List<Group> GetAllGroupsAndFill()
        {
            List<Group> result = _context
                .CreateSimple("select * from [mon].[Group] with(nolock)")
                .ExecuteQuery<Group>()
                .ToList();

            Dictionary<short, Group> dic = new Dictionary<short, Group>();
            foreach (var it in result)
                dic.Add(it.ID, it);

            var proto = new { GroupID = default(short), InstanceID = default(int) };
            var grInstances = _context
                .CreateSimple("select GroupID, InstanceID from [mon].[GroupInstance] with(nolock)")
                .ExecuteQueryAnonymous(proto);

            foreach (var it in grInstances)
                if (dic.ContainsKey(it.GroupID))
                    dic[it.GroupID].Instances.Add(it.InstanceID);

            return result;
        }

        public void CreateNewSource(Source src)
        {
            src.ID = _context.InsertAndGetId<Source, short>("[mon].[Source]", src);
        }

        public void CreateNewInstance(Instance ins)
        {
            ins.ID = _context.InsertAndGetId<Instance, int>("[mon].[Instance]", ins);
        }

        public void AddInstanceToGroup(int iId, short gId)
        {
            var value = new { GroupID = gId, InstanceID = iId };
            _context.Insert("[mon].[GroupInstance]", value, "ID");
        }

        public void RemoveInstanceFromGroup(int iId, short gId)
        {
            _context
                .CreateSimple(@"delete from [mon].[GroupInstance] where InstanceID = @p0", iId)
                .ExecuteNonQuery();
        }

        public short CreateGroup(Group_ group)
        {
            return _context.InsertAndGetId<Group_, short>("[mon].[Group]", group);
        }

        public void RemoveGroup(short id)
        {
            const string query = @"
delete from [mon].[Group] where ID = @p0
delete from [mon].[GroupInstance] where GroupID = @p0
";
            _context
                .CreateSimple(query, id)
                .ExecuteNonQuery();
        }

        public long GetMaxLogId()
        {
            var result = _context.CreateSimple("select max(ID) from [mon].[Log] with(nolock)").ExecuteScalar();
            return result == DBNull.Value ? 0 : (long)result;
        }

        public long GetMaxKeepAliveId()
        {
            var result = _context.CreateSimple("select max(ID) from [mon].[KeepAlive] with(nolock)").ExecuteScalar();
            return result == DBNull.Value ? 0 : (long)result;
        }

        public List<Log_> GetLastLogs(int top)
        {
            return _context
                .CreateSimple($"select top {top} * from [mon].[Log] with(nolock) order by ID desc")
                .ExecuteQuery<Log_>()
                .OrderBy(x => x.ID)
                .ToList();
        }

        public List<KeepAlive_> GetLastKeepAlive(int top)
        {
            return _context
                .CreateSimple($"select top {top} * from [mon].[KeepAlive] with(nolock) order by ID desc")
                .ExecuteQuery<KeepAlive_>()
                .ToList();
        }

        public long? GetLogThreshold(int dayDeep)
        {
            var logThreshold = _context
                .CreateSimple("select max(LastLogID) from mon.HourStat with(nolock) where Hour < DATEADD(DAY, -@p0, GETDATE())", dayDeep)
                .ExecuteScalar();
            return logThreshold == DBNull.Value ? null : (long?)logThreshold;
        }

        public long? GetKeepAliveThreshold(int dayDeep)
        {
            var kaThreshold = _context
                .CreateSimple("select max(LastKeepAliveID) from mon.HourStat with(nolock) where Hour < DATEADD(DAY, -@p0, GETDATE())", dayDeep)
                .ExecuteScalar();
            return kaThreshold == DBNull.Value ? null : (long?)kaThreshold;
        }

        public int CleanUpLog(long lastLog)
        {
            return _context.CleanUpInBatches("mon.Log", lastLog, _settings.CleanupBatchSize);
        }

        public int CleanUpKeepAlive(long lastKeepAlive)
        {
            return _context.CleanUpInBatches("mon.KeepAlive", lastKeepAlive, _settings.CleanupBatchSize);
        }

        public void CreateHourStat(DateTime hour, long lastLogId, long lastKeepAliveId)
        {
            var stat = new { Hour = hour, LastLogID = lastLogId, LastKeepAliveID = lastKeepAliveId };
            _context.Insert("[mon].[HourStat]", stat);
        }

        public void WriteLogs(IEnumerable<Log_> values)
        {
            values.WriteToServer(_context, "[mon].[Log]", _bulkOptions);
        }

        public void WriteKeepAlives(IEnumerable<KeepAlive_> values)
        {
            values.WriteToServer(_context, "[mon].[KeepAlive]", _bulkOptions);
        }

        public List<EventQueue> GetEventSources()
        {
            return _context
                .CreateSimple("select * from [mon].[EventQueue] with(nolock)")
                .ExecuteQuery<EventQueue>()
                .ToList();
        }

        public Metric_ CreateMetric(string name, int aggregation, int instanceId)
        {
            var firstId = _context.InsertAndGetId<Measure_, long>("mon.Measure", new Measure_ { ID = 0, Value = 0 });

            const string fillScript = @"DECLARE @i int = 0;

WHILE @i <= 3999 -- insert n rows.  change this value to whatever you want.
BEGIN

insert [mon].[Measure] values (0)
SET @i = @i + 1;

END";

            _context
                .CreateSimple(fillScript)
                .ExecuteNonQuery();

            var met = new Metric_
            {
                Name = name,
                Aggregation = aggregation,
                InstanceID = instanceId,

                RangeHeadID = firstId,
                RangeTailID = firstId + 4000,

                ActualInterval = DateTime.UtcNow.RoundUp(TimeSpan.FromMinutes(5)),
                ActualID = firstId
            };

            met.ID = _context.InsertAndGetId<Metric_, int>("mon.Metric", met);

            return met;
        }

        public Metric_ GetMetric(int metricId)
        {
            return _context
                .CreateSimple("select * from mon.Metric with(nolock) where ID = @p0", metricId)
                .ExecuteQuery<Metric_>()
                .First();
        }

        public Measure_[] GetMeasures(int metricId)
        {
            const string query = @"
select meas.*
from mon.Measure meas with(nolock)
join mon.Metric met with(nolock) on met.RangeHeadID <= meas.ID and met.RangeTailID >= meas.ID
where met.ID = @p0
order by meas.ID
";
            return _context
                .CreateSimple(query, metricId)
                .ExecuteQuery<Measure_>()
                .ToArray();
        }

        public void SaveMetric(Metric_ metric, Measure_[] measures)
        {
            _context
                .CreateUpdate("mon.Metric", metric, "ID")
                .ExecuteNonQuery();

            foreach (var meas in measures)
                _context
                    .CreateUpdate("mon.Measure", meas, "ID")
                    .ExecuteNonQuery();
        }

        public void RemoveMetric(int id)
        {
            const string query = @"
begin transaction;

	declare @measuresRange table ( RangeHeadID bigint, RangeTailID bigint );
	
	delete met
	output deleted.RangeHeadID, deleted.RangeTailID	into @measuresRange
	from [mon].[Metric] met
	where met.ID = @p0

	delete meas
	from [mon].[Measure] meas
		join @measuresRange r on r.RangeHeadID <= meas.ID and r.RangeTailID >= meas.ID

commit transaction;
";
            _context
                .CreateSimple(query, id)
                .ExecuteNonQuery();
        }

        public void RemoveInstance(int id)
        {
            const string query = @"
delete from [mon].[Instance] where ID = @p0
delete from [mon].[GroupInstance] where InstanceID = @p0
";
            _context
                .CreateSimple(query, id)
                .ExecuteNonQuery();
        }

        public void RemoveSource(short id)
        {
            _context
                .CreateSimple("delete from [mon].[Source] where ID = @p0", id)
                .ExecuteNonQuery();
        }

        public int[] GetAllMetricIds()
        {
            var proto = new { ID = 0 };

            return _context
                .CreateSimple("select ID from mon.Metric with(nolock)")
                .ExecuteQueryAnonymous(proto)
                .Select(x => x.ID)
                .ToArray();
        }
    } //end of class
}
