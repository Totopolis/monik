using System;
using System.Collections.Generic;
using System.Linq;
using Gerakul.FastSql.Common;
using Monik.Common;

namespace Monik.Service
{
    public class RepositoryPostgreSql : IRepository
    {
        private readonly IMonikServiceSettings _settings;
        private readonly DbContext _context;
        private readonly BulkOptions _bulkOptions;

        public RepositoryPostgreSql(IMonikServiceSettings settings, ContextProvider provider)
        {
            _settings = settings;
            _context = provider.CreateContext(_settings.DbConnectionString);

            _bulkOptions = provider.BulkOptions;
        }

        public List<Source> GetAllSources()
        {
            return _context
                .CreateSimple("select * from \"mon\".\"Source\"")
                .ExecuteQuery<Source>()
                .ToList();
        }

        public List<Instance> GetAllInstances()
        {
            return _context
                .CreateSimple("select * from \"mon\".\"Instance\"")
                .ExecuteQuery<Instance>()
                .ToList();
        }

        public List<Group> GetAllGroupsAndFill()
        {
            List<Group> result = _context
                .CreateSimple("select * from \"mon\".\"Group\"")
                .ExecuteQuery<Group>()
                .ToList();

            Dictionary<short, Group> dic = new Dictionary<short, Group>();
            foreach (var it in result)
                dic.Add(it.ID, it);

            var proto = new { GroupID = default(short), InstanceID = default(int) };
            var grInstances = _context
                .CreateSimple("select \"GroupID\", \"InstanceID\" from \"mon\".\"GroupInstance\"")
                .ExecuteQueryAnonymous(proto);

            foreach (var it in grInstances)
                if (dic.ContainsKey(it.GroupID))
                    dic[it.GroupID].Instances.Add(it.InstanceID);

            return result;
        }

        public void CreateNewSource(Source src)
        {
            src.ID = _context.InsertAndGetId<Source, short>("\"mon\".\"Source\"", src);
        }

        public void CreateNewInstance(Instance ins)
        {
            ins.ID = _context.InsertAndGetId<Instance, int>("\"mon\".\"Instance\"", ins);
        }

        public void AddInstanceToGroup(int iId, short gId)
        {
            var value = new { GroupID = gId, InstanceID = iId };
            _context.Insert("\"mon\".\"GroupInstance\"", value, "ID");
        }

        public void RemoveInstanceFromGroup(int iId, short gId)
        {
            _context
                .CreateSimple("delete from \"mon\".\"GroupInstance\" where \"InstanceID\" = @p0", iId)
                .ExecuteNonQuery();
        }

        public short CreateGroup(Group_ group)
        {
            return _context.InsertAndGetId<Group_, short>("\"mon\".\"Group\"", group);
        }

        public void RemoveGroup(short id)
        {
            const string query = @"
delete from ""mon"".""Group"" where ""ID"" = @p0
delete from ""mon"".""GroupInstance"" where ""GroupID"" = @p0
";
            _context
                .CreateSimple(query, id)
                .ExecuteNonQuery();
        }

        public long GetMaxLogId()
        {
            var result = _context.CreateSimple("select max(\"ID\") from \"mon\".\"Log\"").ExecuteScalar();
            return result == DBNull.Value ? 0 : (long)result;
        }

        public long GetMaxKeepAliveId()
        {
            var result = _context.CreateSimple("select max(\"ID\") from \"mon\".\"KeepAlive\"").ExecuteScalar();
            return result == DBNull.Value ? 0 : (long)result;
        }

        public List<Log_> GetLastLogs(int top)
        {
            return _context
                .CreateSimple($"select * from \"mon\".\"Log\" order by \"ID\" desc limit {top}")
                .ExecuteQuery<Log_>()
                .OrderBy(x => x.ID)
                .ToList();
        }

        public List<KeepAlive_> GetLastKeepAlive(int top)
        {
            return _context
                .CreateSimple($"select * from \"mon\".\"KeepAlive\" order by \"ID\" desc limit {top}")
                .ExecuteQuery<KeepAlive_>()
                .ToList();
        }

        public long? GetLogThreshold(int dayDeep)
        {
            var logThreshold = _context
                .CreateSimple("select max(\"LastLogID\") from \"mon\".\"HourStat\" where \"Hour\" < CURRENT_DATE - @p0 * INTERVAL '1 day'", dayDeep)
                .ExecuteScalar();
            return logThreshold == DBNull.Value ? null : (long?)logThreshold;
        }

        public long? GetKeepAliveThreshold(int dayDeep)
        {
            var kaThreshold = _context
                .CreateSimple("select max(\"LastKeepAliveID\") from \"mon\".\"HourStat\" where \"Hour\" < CURRENT_DATE - @p0 * INTERVAL '1 day'", dayDeep)
                .ExecuteScalar();
            return kaThreshold == DBNull.Value ? null : (long?)kaThreshold;
        }

        public int CleanUpLog(long lastLog)
        {
            return _context.CleanUpInBatches("\"mon\".\"Log\"", lastLog, _settings.CleanupBatchSize);
        }

        public int CleanUpKeepAlive(long lastKeepAlive)
        {
            return _context.CleanUpInBatches("\"mon\".\"KeepAlive\"", lastKeepAlive, _settings.CleanupBatchSize);
        }

        public void CreateHourStat(DateTime hour, long lastLogId, long lastKeepAliveId)
        {
            var stat = new { Hour = hour, LastLogID = lastLogId, LastKeepAliveID = lastKeepAliveId };
            _context.Insert("\"mon\".\"HourStat\"", stat);
        }

        public void WriteLogs(IEnumerable<Log_> values)
        {
            values.WriteToServer(_context, "\"mon\".\"Log\"", _bulkOptions);
        }

        public void WriteKeepAlives(IEnumerable<KeepAlive_> values)
        {
            values.WriteToServer(_context, "\"mon\".\"KeepAlive\"", _bulkOptions);
        }

        public List<EventQueue> GetEventSources()
        {
            return _context
                .CreateSimple("select * from \"mon\".\"EventQueue\"")
                .ExecuteQuery<EventQueue>()
                .ToList();
        }

        public Metric_ CreateMetric(string name, int aggregation, int instanceId)
        {
            var firstId = _context.InsertAndGetId<Measure_, long>("\"mon\".\"Measure\"", new Measure_ { ID = 0, Value = 0 });

            _context
                .CreateSimple("INSERT INTO \"mon\".\"Measure\" (\"Value\") SELECT 0 FROM generate_series(1, 4000);")
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

            met.ID = _context.InsertAndGetId<Metric_, int>("\"mon\".\"Metric\"", met);

            return met;
        }

        public Metric_ GetMetric(int metricId)
        {
            return _context
                .CreateSimple("select * from \"mon\".\"Metric\" where \"ID\" = @p0", metricId)
                .ExecuteQuery<Metric_>()
                .First();
        }

        public Measure_[] GetMeasures(int metricId)
        {
            const string query = @"
select meas.*
from ""mon"".""Measure"" meas
join ""mon"".""Metric"" met on met.""RangeHeadID"" <= meas.""ID"" and met.""RangeTailID"" >= meas.""ID""
where met.""ID"" = @p0
order by meas.""ID""
";
            return _context
                .CreateSimple(query, metricId)
                .ExecuteQuery<Measure_>()
                .ToArray();
        }

        public void SaveMetric(Metric_ metric, Measure_[] measures)
        {
            _context
                .CreateUpdate("\"mon\".\"Metric\"", metric, "ID")
                .ExecuteNonQuery();

            foreach (var meas in measures)
                _context
                    .CreateUpdate("\"mon\".\"Measure\"", meas, "ID")
                    .ExecuteNonQuery();
        }

        public void RemoveMetric(int id)
        {
            const string query = @"
WITH removed AS (
    delete from ""mon"".""Metric"" met
    where met.""ID"" = @p0
    returning ""RangeHeadID"", ""RangeTailID""
)
delete from ""mon"".""Measure"" meas
using removed r
where r.""RangeHeadID"" <= meas.""ID"" and r.""RangeTailID"" >= meas.""ID""
";
            _context
                .CreateSimple(query, id)
                .ExecuteNonQuery();
        }

        public void RemoveInstance(int id)
        {
            const string query = @"
delete from ""mon"".""Instance"" where ""ID"" = @p0;
delete from ""mon"".""GroupInstance"" where ""InstanceID"" = @p0;
";
            _context
                .CreateSimple(query, id)
                .ExecuteNonQuery();
        }

        public void RemoveSource(short id)
        {
            _context
                .CreateSimple("delete from \"mon\".\"Source\" where \"ID\" = @p0", id)
                .ExecuteNonQuery();
        }

        public int[] GetAllMetricIds()
        {
            var proto = new { ID = 0 };

            return _context
                .CreateSimple("select \"ID\" from \"mon\".\"Metric\"")
                .ExecuteQueryAnonymous(proto)
                .Select(x => x.ID)
                .ToArray();
        }
    } //end of class
}
