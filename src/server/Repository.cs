using Gerakul.FastSql;
using Monik.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Monik.Service
{
    internal static class RepositoryHelper
    {
        public static int CleanUpInBatches(string connectionString, string tableName, long maxId, int batchSize)
        {
            var total = 0;
            var stopWatch = new System.Diagnostics.Stopwatch();
            while (true)
            {
                stopWatch.Restart();
                var deleted = SimpleCommand.ExecuteNonQuery(connectionString,
                    $"delete top ({batchSize}) from {tableName} with (readpast) where ID < @p0", maxId);
                stopWatch.Stop();

                total += deleted;

                if (deleted > 0)
                    System.Threading.Tasks.Task.Delay((int)stopWatch.ElapsedMilliseconds).Wait();
                else
                    break;
            }
            return total;
        }

        public static void WriteInBulk<T>(IEnumerable<T> values, string connectionString, string tableName, int? batchSize = null, int? timeout = null)
        {
            values.WriteToServer(
                new BulkOptions(batchSize, timeout, enableStreaming: true),
                connectionString, tableName);
        }
    }

    public class Repository : IRepository
    {
        private readonly IMonikServiceSettings _settings;

        public Repository(IMonikServiceSettings settings)
        {
            _settings = settings;
        }

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

            var proto = new { GroupID = default(short), InstanceID = default(int) };
            var grInstances = SimpleCommand.ExecuteQueryAnonymous(proto, _settings.DbConnectionString,
                "select GroupID, InstanceID from [mon].[GroupInstance]");

            foreach (var it in grInstances)
                if (dic.ContainsKey(it.GroupID))
                    dic[it.GroupID].Instances.Add(it.InstanceID);

            return result;
        }

        public void CreateNewSource(Source src)
        {
            src.ID = (short)MappedCommand.InsertAndGetId<Source>(_settings.DbConnectionString, "[mon].[Source]", src, "ID");
        }

        public void CreateNewInstance(Instance ins)
        {
            ins.ID = (int)MappedCommand.InsertAndGetId<Instance>(_settings.DbConnectionString, "[mon].[Instance]", ins, "ID");
        }

        public void AddInstanceToGroup(Instance ins, Group group)
        {
            var proto = new { GroupID = group.ID, InstanceID = ins.ID };
            MappedCommand.Insert(_settings.DbConnectionString, "mon.GroupInstance", proto, "ID");
        }

        public long GetMaxLogId()
        {
            var result = SimpleCommand.ExecuteScalar(_settings.DbConnectionString, "select max(ID) from [mon].[Log]");
            return result == System.DBNull.Value ? 0 : (long)result;
        }

        public long GetMaxKeepAliveId()
        {
            var result = SimpleCommand.ExecuteScalar(_settings.DbConnectionString, "select max(ID) from [mon].[KeepAlive]");
            return result == System.DBNull.Value ? 0 : (long)result;
        }

        public List<Log_> GetLastLogs(int top)
        {
            return
                SimpleCommand.ExecuteQuery<Log_>(_settings.DbConnectionString,
                    $"select top {top} * from [mon].[Log] order by ID desc").OrderBy(x => x.ID).ToList();
        }

        public List<KeepAlive_> GetLastKeepAlive(int top)
        {
            return
                SimpleCommand.ExecuteQuery<KeepAlive_>(_settings.DbConnectionString,
                    $"select top {top} * from [mon].[KeepAlive] order by ID desc").ToList();
        }

        public long? GetLogThreshold(int dayDeep)
        {
            var logThreshold = SimpleCommand.ExecuteScalar(_settings.DbConnectionString,
                "select max(LastLogID) from mon.HourStat where Hour < DATEADD(DAY, -@p0, GETDATE())", dayDeep);
            return logThreshold == System.DBNull.Value ? null : (long?)logThreshold;
        }

        public long? GetKeepAliveThreshold(int dayDeep)
        {
            var kaThreshold = SimpleCommand.ExecuteScalar(_settings.DbConnectionString,
                "select max(LastKeepAliveID) from mon.HourStat where Hour < DATEADD(DAY, -@p0, GETDATE())", dayDeep);
            return kaThreshold == System.DBNull.Value ? null : (long?)kaThreshold;
        }

        public int CleanUpLog(long lastLog)
        {
            return RepositoryHelper.CleanUpInBatches(_settings.DbConnectionString, "mon.Log", lastLog, _settings.CleanupBatchSize);
        }

        public int CleanUpKeepAlive(long lastKeepAlive)
        {
            return RepositoryHelper.CleanUpInBatches(_settings.DbConnectionString, "mon.KeepAlive", lastKeepAlive, _settings.CleanupBatchSize);
        }

        public void CreateHourStat(DateTime hour, long lastLogId, long lastKeepAliveId)
        {
            var stat = new { Hour = hour, LastLogID = lastLogId, LastKeepAliveID = lastKeepAliveId };
            MappedCommand.Insert(_settings.DbConnectionString, "[mon].[HourStat]", stat);
        }

        public void WriteLogs(IEnumerable<Log_> values)
        {
            RepositoryHelper.WriteInBulk(values, _settings.DbConnectionString, "[mon].[Log]",
                _settings.WriteBatchSize, _settings.WriteBatchTimeout);
        }

        public void WriteKeepAlives(IEnumerable<KeepAlive_> values)
        {
            RepositoryHelper.WriteInBulk(values, _settings.DbConnectionString, "[mon].[KeepAlive]",
                _settings.WriteBatchSize, _settings.WriteBatchTimeout);
        }

        public List<EventQueue> GetEventSources()
        {
            return SimpleCommand.ExecuteQuery<EventQueue>(_settings.DbConnectionString, "select * from [mon].[EventQueue]").ToList();
        }

        public Metric_ CreateMetric(string name, int aggregation, int instanceId)
        {
            var measures = Enumerable.Range(1, 3999)
                .Select(x => new Measure_ { ID = 0, Value = 0 })
                .ToArray();

            var firstId = (long)MappedCommand.InsertAndGetId(_settings.DbConnectionString,
                "mon.Measure", measures[0], "ID");

            string fillScript = @"DECLARE @i int = 0;

WHILE @i <= 3999 -- insert n rows.  change this value to whatever you want.
BEGIN

insert [mon].[Measure] values (0)
SET @i = @i + 1;

END";

            SimpleCommand.ExecuteNonQuery(_settings.DbConnectionString, fillScript);

            // measures.WriteToServer(_settings.DbConnectionString, "mon.Measure");

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

            met.ID = (int)MappedCommand.InsertAndGetId(_settings.DbConnectionString, "mon.Metric", met, "ID");
            return met;
        }

        public Metric_ GetMetric(int metricId)
        {
            var q1 = SimpleCommand.ExecuteQuery<Metric_>(_settings.DbConnectionString,
                $"select * from mon.Metric where ID={metricId}");

            return q1.First();
        }

        public Measure_[] GetMeasures(int metricId)
        {
            var q1 = SimpleCommand.ExecuteQuery<Measure_>(_settings.DbConnectionString,
$@"
select meas.*
from mon.Measure meas
join mon.Metric met on met.RangeHeadID <= meas.ID and met.RangeTailID >= meas.ID
where met.ID = {metricId}
order by meas.ID");

            return q1.ToArray();
        }

        public void SaveMetric(Metric_ metric, Measure_[] measures)
        {
            MappedCommand.Update(_settings.DbConnectionString, "mon.Metric", metric, "ID");

            foreach (var meas in measures)
                MappedCommand.Update(_settings.DbConnectionString, "mon.Measure", meas, "ID");
        }

        public int[] GetAllMetricIds()
        {
            var proto = new { ID = 0 };

            var result = SimpleCommand
                .ExecuteQueryAnonymous(proto, _settings.DbConnectionString, "select ID from mon.Metric")
                .Select(x => x.ID);

            return result.ToArray();
        }
    } //end of class
}
