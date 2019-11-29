using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Monik.Common;

namespace Monik.Service
{
    public class RepositorySqlServer : RepositoryBase, IRepository
    {
        private readonly IMonikServiceSettings _settings;

        public RepositorySqlServer(IMonikServiceSettings settings)
        {
            _settings = settings;
        }

        protected override IDbConnection Connection => new SqlConnection(_settings.DbConnectionString);

        public List<Source> GetAllSources()
        {
            using (var con = Connection)
            {
                return con
                    .Query<Source>("select * from [mon].[Source] with(nolock)")
                    .ToList();
            }
        }

        public List<Instance> GetAllInstances()
        {
            using (var con = Connection)
            {
                return con
                    .Query<Instance>("select * from [mon].[Instance] with(nolock)")
                    .ToList();
            }
        }

        public List<Group> GetAllGroupsAndFill()
        {
            const string query = @"
select * from [mon].[Group] with(nolock);
select GroupID, InstanceID from [mon].[GroupInstance] with(nolock);
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
insert into [mon].[Source]
([Created], [Name], [Description], [DefaultGroupID])
values
(@Created, @Name, @Description, @DefaultGroupID)
;select SCOPE_IDENTITY() id
";

            using (var con = Connection)
            {
                src.ID = con.ExecuteScalar<short>(query, src);
            }
        }

        public void CreateNewInstance(Instance ins)
        {
            const string query = @"
insert into [mon].[Instance]
([Created], [SourceID], [Name], [Description])
values
(@Created, @SourceID, @Name, @Description)
;select SCOPE_IDENTITY() id
";

            using (var con = Connection)
            {
                ins.ID = con.ExecuteScalar<int>(query, ins);
            }
        }

        public void AddInstanceToGroup(int iId, short gId)
        {
            const string query = @"
insert into [mon].[GroupInstance]
([GroupID], [InstanceID])
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
            const string query = @"delete from [mon].[GroupInstance] where InstanceID = @InstanceId";

            using (var con = Connection)
            {
                con.Execute(query, new {InstanceId = iId});
            }
        }

        public short CreateGroup(Group_ group)
        {
            const string query = @"
insert into [mon].[Group]
([Name], [IsDefault], [Description])
values
(@Name, @IsDefault, @Description)
;select SCOPE_IDENTITY() id
";

            using (var con = Connection)
            {
                return con.ExecuteScalar<short>(query, group);
            }
        }

        public void RemoveGroup(short id)
        {
            const string query = @"
delete from [mon].[Group] where ID = @Id;
delete from [mon].[GroupInstance] where GroupID = @Id;
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
                return con.QueryFirstOrDefault<long>("select max(ID) from [mon].[Log] with(nolock)");
            }
        }

        public long GetMaxKeepAliveId()
        {
            using (var con = Connection)
            {
                return con.QueryFirstOrDefault<long>("select max(ID) from [mon].[KeepAlive] with(nolock)");
            }
        }

        public List<Log_> GetLastLogs(int top)
        {
            const string query = "select top (@Top) * from [mon].[Log] with(nolock) order by ID desc";

            using (var con = Connection)
            {
                return con.Query<Log_>(query, new {Top = top})
                    .OrderBy(x => x.ID)
                    .ToList();
            }
        }

        public List<KeepAlive_> GetLastKeepAlive(int top)
        {
            const string query = @"
select top (@Top) *
from [mon].[KeepAlive] with(nolock)
order by ID desc
";

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
select max(LastLogID)
from [mon].[HourStat] with(nolock)
where Hour < DATEADD(DAY, -@DayDeep, GETDATE())
";

            using (var con = Connection)
            {
                return con.ExecuteScalar<long?>(query, new {DayDeep = dayDeep});
            }
        }

        public long? GetKeepAliveThreshold(int dayDeep)
        {
            const string query = @"
select max(LastKeepAliveID)
from [mon].[HourStat] with(nolock)
where Hour < DATEADD(DAY, -@DayDeep, GETDATE())
";

            using (var con = Connection)
            {
                return con.ExecuteScalar<long?>(query, new {DayDeep = dayDeep});
            }
        }

        public int CleanUpLog(long lastLog)
        {
            const string query = @"
delete top (@BatchSize)
from [mon].[Log] with (readpast)
where ID < @Id
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
delete top (@BatchSize)
from [mon].[KeepAlive] with (readpast)
where ID < @Id
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
insert into [mon].[HourStat]
([Hour], [LastLogID], [LastKeepAliveID])
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
            using (var con = (SqlConnection) Connection)
            using (var bcp = new SqlBulkCopy(con))
            {
                bcp.DestinationTableName = "[mon].[Log]";
                bcp.BatchSize = _settings.WriteBatchSize;
                bcp.BulkCopyTimeout = _settings.WriteBatchTimeout;

                var table = new DataTable("[mon].[Log]");
                table.Columns.Add("[ID]");
                table.Columns.Add("[Created]");
                table.Columns.Add("[Received]");
                table.Columns.Add("[Level]");
                table.Columns.Add("[Severity]");
                table.Columns.Add("[InstanceID]");
                table.Columns.Add("[Format]");
                table.Columns.Add("[Body]");
                table.Columns.Add("[Tags]");

                foreach (var item in values)
                {
                    table.Rows.Add(item.ID,
                        item.Created,
                        item.Received,
                        item.Level,
                        item.Severity,
                        item.InstanceID,
                        item.Format,
                        item.Body,
                        item.Tags);
                }

                con.Open();
                bcp.WriteToServer(table);
            }
        }

        public void WriteKeepAlives(IEnumerable<KeepAlive_> values)
        {
            using (var con = (SqlConnection) Connection)
            using (var bcp = new SqlBulkCopy(con))
            {
                bcp.DestinationTableName = "[mon].[KeepAlive]";
                bcp.BatchSize = _settings.WriteBatchSize;
                bcp.BulkCopyTimeout = _settings.WriteBatchTimeout;

                var table = new DataTable("[mon].[KeepAlive]");
                table.Columns.Add("[ID]");
                table.Columns.Add("[Created]");
                table.Columns.Add("[Received]");
                table.Columns.Add("[InstanceID]");

                foreach (var item in values)
                {
                    table.Rows.Add(item.ID, item.Created, item.Received, item.InstanceID);
                }

                con.Open();
                bcp.WriteToServer(table);
            }
        }

        public Metric_ CreateMetric(string name, int aggregation, int instanceId)
        {
            const string insertMeasures = @"
begin transaction;

EXEC sp_getapplock @Resource = '[mon].[Measure]', @LockMode = 'Exclusive';  

declare @i int = 1;
while (@i <= @Count)
begin
     insert into [mon].[Measure] ([Value]) values (0)
     set @i = @i + 1
end
;select SCOPE_IDENTITY() id

commit transaction;
";
            const string insertMetric = @"
insert into [mon].[Metric]
([Name], [InstanceID], [Aggregation], [RangeHeadID], [RangeTailID], [ActualInterval], [ActualID])
VALUES
(@Name, @InstanceID, @Aggregation, @RangeHeadID, @RangeTailID, @ActualInterval, @ActualID)
;select SCOPE_IDENTITY() id    
";

            using (var con = Connection)
            {
                var lastId = con.ExecuteScalar<long>(insertMeasures, new {Count = MeasuresPerMetric});
                var firstId = lastId - MeasuresPerMetric + 1;
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
                    "select * from [mon].[Metric] with(nolock) where ID = @Id",
                    new {Id = metricId});
            }
        }

        public Measure_[] GetMeasures(int metricId)
        {
            const string query = @"
select meas.*
from mon.Measure meas with(nolock)
join mon.Metric met with(nolock) on met.RangeHeadID <= meas.ID and met.RangeTailID >= meas.ID
where met.ID = @Id
order by meas.ID
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
UPDATE [mon].[Metric] SET
    Name = @Name
    ,InstanceID = @InstanceID
    ,Aggregation = @Aggregation
    
    ,RangeHeadID = @RangeHeadID
    ,RangeTailID = @RangeTailID
    
    ,ActualInterval = @ActualInterval
    ,ActualID = @ActualID
WHERE ID = @ID
";

            const string updateMeasure = @"
UPDATE [mon].[Measure] SET
    Value = @Value
WHERE ID = @ID
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
begin transaction;

	declare @measuresRange table ( RangeHeadID bigint, RangeTailID bigint );
	
	delete met
	output deleted.RangeHeadID, deleted.RangeTailID	into @measuresRange
	from [mon].[Metric] met
	where met.ID = @Id

	delete meas
	from [mon].[Measure] meas
		join @measuresRange r on r.RangeHeadID <= meas.ID and r.RangeTailID >= meas.ID

commit transaction;
";

            using (var con = Connection)
            {
                con.Execute(query, new {Id = id});
            }
        }

        public void RemoveInstance(int id)
        {
            const string query = @"
delete from [mon].[Instance] where ID = @Id
delete from [mon].[GroupInstance] where InstanceID = @Id
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
                con.Execute("delete from [mon].[Source] where ID = @Id", new {Id = id});
            }
        }

        public int[] GetAllMetricIds()
        {
            using (var con = Connection)
            {
                return con
                    .Query<int>("select ID from [mon].[Metric] with(nolock)")
                    .ToArray();
            }
        }
    } //end of class
}