using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Monik.Common;
using Npgsql;

namespace Monik.Service
{
    public class RepositoryPostgreSql : RepositoryBase, IRepository
    {
        private const int MeasuresCreationLockId = 111;

        private readonly IMonikServiceSettings _settings;

        public RepositoryPostgreSql(IMonikServiceSettings settings)
        {
            _settings = settings;
        }

        protected override IDbConnection Connection => new NpgsqlConnection(_settings.DbConnectionString);

        public List<Source> GetAllSources()
        {
            using (var con = Connection)
            {
                return con
                    .Query<Source>("select * from mon.\"Source\"")
                    .ToList();
            }
        }

        public List<Instance> GetAllInstances()
        {
            using (var con = Connection)
            {
                return con
                    .Query<Instance>("select * from mon.\"Instance\"")
                    .ToList();
            }
        }

        public List<Group> GetAllGroupsAndFill()
        {
            const string query = @"
select * from mon.""Group"";
select ""GroupID"", ""InstanceID"" from mon.""GroupInstance"";
";

            List<Group> groups;
            List<dynamic> grInstances;
            using (var con = Connection)
            using (var multi = con.QueryMultiple(query))
            {
                groups = multi.Read<Group>().ToList();
                grInstances = multi.Read<dynamic>().ToList();
            }

            var groupDic = groups.ToDictionary(g => g.ID);
            foreach (var it in grInstances.Where(it => groupDic.ContainsKey(it.GroupID)))
                groupDic[it.GroupID].Instances.Add(it.InstanceID);

            return groups;
        }

        public void CreateNewSource(Source src)
        {
            const string query = @"
insert into mon.""Source""
(""Created"", ""Name"", ""Description"", ""DefaultGroupID"")
values
(@Created, @Name, @Description, @DefaultGroupID)
returning ""ID""
";

            using (var con = Connection)
            {
                src.ID = con.ExecuteScalar<short>(query, src);
            }
        }

        public void CreateNewInstance(Instance ins)
        {
            const string query = @"
insert into mon.""Instance""
(""Created"", ""SourceID"", ""Name"", ""Description"")
values
(@Created, @SourceID, @Name, @Description)
returning ""ID""
";

            using (var con = Connection)
            {
                ins.ID = con.ExecuteScalar<int>(query, ins);
            }
        }

        public void AddInstanceToGroup(int iId, short gId)
        {
            const string query = @"
insert into mon.""GroupInstance""
(""GroupID"", ""InstanceID"")
values
(@GroupID, @InstanceID)
";

            using (var con = Connection)
            {
                con.Execute(query, new {GroupID = gId, InstanceID = iId});
            }
        }

        public void RemoveInstanceFromGroup(int iId, short gId)
        {
            const string query = "delete from mon.\"GroupInstance\" where \"InstanceID\" = @InstanceId";

            using (var con = Connection)
            {
                con.Execute(query, new {InstanceId = iId});
            }
        }

        public short CreateGroup(Group_ group)
        {
            const string query = @"
insert into mon.""Group""
(""Name"", ""IsDefault"", ""Description"")
values
(@Name, @IsDefault, @Description)
returning ""ID""
";

            using (var con = Connection)
            {
                return con.ExecuteScalar<short>(query, group);
            }
        }

        public void RemoveGroup(short id)
        {
            const string query = @"
delete from mon.""Group"" where ""ID"" = @Id;
delete from mon.""GroupInstance"" where ""GroupID"" = @Id;
";

            using (var con = Connection)
            {
                con.Execute(query, new {Id = id});
            }
        }

        public long GetMaxLogId()
        {
            using (var con = Connection)
            {
                return con.QueryFirstOrDefault<long>("select \"ID\" from mon.\"Log\" order by \"ID\" desc");
            }
        }

        public long GetMaxKeepAliveId()
        {
            using (var con = Connection)
            {
                return con.QueryFirstOrDefault<long>("select \"ID\" from mon.\"KeepAlive\" order by \"ID\" desc");
            }
        }

        public List<Log_> GetLastLogs(int top)
        {
            const string query = "select * from mon.\"Log\" order by \"ID\" desc limit @Top";

            using (var con = Connection)
            {
                return con.Query<Log_>(query, new {Top = top})
                    .OrderBy(x => x.ID)
                    .ToList();
            }
        }

        public List<KeepAlive_> GetLastKeepAlive(int top)
        {
            const string query = "select * from mon.\"KeepAlive\" order by \"ID\" desc limit @Top";

            using (var con = Connection)
            {
                return con.Query<KeepAlive_>(query, new {Top = top})
                    .OrderBy(x => x.ID)
                    .ToList();
            }
        }

        public long? GetLogThreshold(int dayDeep)
        {
            const string query = @"
select max(""LastLogID"")
from mon.""HourStat""
where ""Hour"" < CURRENT_DATE - @DayDeep * INTERVAL '1 day'
";

            using (var con = Connection)
            {
                return con.ExecuteScalar<long?>(query, new {DayDeep = dayDeep});
            }
        }

        public long? GetKeepAliveThreshold(int dayDeep)
        {
            const string query = @"
select max(""LastKeepAliveID"")
from mon.""HourStat""
where ""Hour"" < CURRENT_DATE - @DayDeep * INTERVAL '1 day'
";

            using (var con = Connection)
            {
                return con.ExecuteScalar<long?>(query, new {DayDeep = dayDeep});
            }
        }

        public int CleanUpLog(long lastLog)
        {
            const string query = @"
delete from mon.""Log""
where ""ID"" in (
    select ""ID""
    from mon.""Log""
    where ""ID"" < @Id
    limit @BatchSize
)
";
            return ExecuteInBatches(query, new
            {
                Id = lastLog,
                BatchSize = _settings.CleanupBatchSize
            });
        }

        public int CleanUpKeepAlive(long lastKeepAlive)
        {
            const string query = @"
delete from mon.""KeepAlive""
where ""ID"" in (
    select ""ID""
    from mon.""KeepAlive""
    where ""ID"" < @Id
    limit @BatchSize
)
";
            return ExecuteInBatches(query, new
            {
                Id = lastKeepAlive,
                BatchSize = _settings.CleanupBatchSize
            });
        }

        public void CreateHourStat(DateTime hour, long lastLogId, long lastKeepAliveId)
        {
            const string query = @"
insert into mon.""HourStat""
(""Hour"", ""LastLogID"", ""LastKeepAliveID"")
values
(@Hour, @LastLogID, @LastKeepAliveID)
";

            using (var con = Connection)
            {
                con.Execute(query, new
                {
                    Hour = hour,
                    LastLogID = lastLogId,
                    LastKeepAliveID = lastKeepAliveId
                });
            }
        }

        public void WriteLogs(IEnumerable<Log_> values)
        {
            const string query = @"
COPY mon.""Log""
(""ID"", ""Created"", ""Received"", ""Level"", ""Severity"", ""InstanceID"", ""Format"", ""Body"", ""Tags"")
FROM STDIN (FORMAT BINARY)
";
            using (var con = (NpgsqlConnection) Connection)
            {
                con.Open();

                using (var writer = con.BeginBinaryImport(query))
                {
                    foreach (var item in values)
                    {
                        writer.StartRow();

                        writer.Write(item.ID);
                        writer.Write(item.Created);
                        writer.Write(item.Received);
                        writer.Write(item.Level);
                        writer.Write(item.Severity);
                        writer.Write(item.InstanceID);
                        writer.Write(item.Format);
                        writer.Write(item.Body);
                        writer.Write(item.Tags);
                    }

                    writer.Complete();
                }
            }
        }

        public void WriteKeepAlives(IEnumerable<KeepAlive_> values)
        {
            const string query = @"
COPY mon.""KeepAlive""
(""ID"", ""Created"", ""Received"", ""InstanceID"")
FROM STDIN (FORMAT BINARY)
";
            using (var con = (NpgsqlConnection) Connection)
            {
                con.Open();

                using (var writer = con.BeginBinaryImport(query))
                {
                    foreach (var item in values)
                    {
                        writer.StartRow();

                        writer.Write(item.ID);
                        writer.Write(item.Created);
                        writer.Write(item.Received);
                        writer.Write(item.InstanceID);
                    }

                    writer.Complete();
                }
            }
        }

        public Metric_ CreateMetric(string name, int aggregation, int instanceId)
        {
            const string insertMeasures = @"
insert into mon.""Measure"" (""Value"")
select 0
from generate_series(1, @Count)
returning ""ID"";
";
            const string insertMetric = @"
insert into mon.""Metric""
(""Name"", ""InstanceID"", ""Aggregation"", ""RangeHeadID"", ""RangeTailID"", ""ActualInterval"", ""ActualID"")
values
(@Name, @InstanceID, @Aggregation, @RangeHeadID, @RangeTailID, @ActualInterval, @ActualID)
returning ""ID""
";

            using (var con = Connection)
            {
                con.Open();

                long[] ids;
                using (var tran = con.BeginTransaction())
                {
                    con.Execute("select pg_advisory_lock(@LockId);",
                        new {LockId = MeasuresCreationLockId}, tran);
                    ids = con
                        .Query<long>(insertMeasures, new {Count = MeasuresPerMetric}, tran)
                        .ToArray();
                    con.Execute("select pg_advisory_unlock(@LockId);",
                        new {LockId = MeasuresCreationLockId}, tran);
                    tran.Commit();
                }

                var lastId = ids.Last();
                var firstId = ids.First();
                var met = new Metric_
                {
                    Name = name,
                    Aggregation = aggregation,
                    InstanceID = instanceId,

                    RangeHeadID = firstId,
                    RangeTailID = lastId,

                    ActualInterval = DateTime.UtcNow.RoundUp(TimeSpan.FromMinutes(5)),
                    ActualID = firstId
                };
                met.ID = con.ExecuteScalar<int>(insertMetric, met);
                return met;
            }
        }

        public Metric_ GetMetric(int metricId)
        {
            using (var con = Connection)
            {
                return con.QuerySingle<Metric_>(
                    "select * from mon.\"Metric\" where \"ID\" = @Id",
                    new {Id = metricId});
            }
        }

        public Measure_[] GetMeasures(int metricId)
        {
            const string query = @"
select meas.*
from mon.""Measure"" meas
join mon.""Metric"" met on met.""RangeHeadID"" <= meas.""ID"" and met.""RangeTailID"" >= meas.""ID""
where met.""ID"" = @Id
order by meas.""ID""
";

            using (var con = Connection)
            {
                return con
                    .Query<Measure_>(query, new {Id = metricId})
                    .ToArray();
            }
        }

        public void SaveMetric(Metric_ metric, Measure_[] measures)
        {
            const string updateMetric = @"
UPDATE mon.""Metric"" SET
    ""Name"" = @Name
    ,""InstanceID"" = @InstanceID
    ,""Aggregation"" = @Aggregation
    
    ,""RangeHeadID"" = @RangeHeadID
    ,""RangeTailID"" = @RangeTailID
    
    ,""ActualInterval"" = @ActualInterval
    ,""ActualID"" = @ActualID
WHERE ""ID"" = @ID
";

            const string updateMeasure = @"
UPDATE mon.""Measure"" SET
    ""Value"" = @Value
WHERE ""ID"" = @ID
";
            using (var con = Connection)
            {
                con.Execute(updateMetric, metric);
                con.Execute(updateMeasure, measures);
            }
        }

        public void RemoveMetric(int id)
        {
            const string query = @"
WITH removed AS (
    delete from mon.""Metric"" met
    where met.""ID"" = @Id
    returning ""RangeHeadID"", ""RangeTailID""
)
delete from mon.""Measure"" meas
using removed r
where r.""RangeHeadID"" <= meas.""ID"" and r.""RangeTailID"" >= meas.""ID""
";

            using (var con = Connection)
            {
                con.Execute(query, new {Id = id});
            }
        }

        public void RemoveInstance(int id)
        {
            const string query = @"
delete from mon.""Instance"" where ""ID"" = @Id;
delete from mon.""GroupInstance"" where ""InstanceID"" = @Id;
";

            using (var con = Connection)
            {
                con.Execute(query, new {Id = id});
            }
        }

        public void RemoveSource(short id)
        {
            using (var con = Connection)
            {
                con.Execute("delete from mon.\"Source\" where \"ID\" = @Id", new {Id = id});
            }
        }

        public int[] GetAllMetricIds()
        {
            using (var con = Connection)
            {
                return con
                    .Query<int>("select \"ID\" from mon.\"Metric\"")
                    .ToArray();
            }
        }
    } //end of class
}