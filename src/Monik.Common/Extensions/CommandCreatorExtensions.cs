using System.Linq;
using Gerakul.FastSql.Common;

namespace Monik.Service
{
    public static partial class CommandCreatorExtensions
    {
        public static int CleanUpInBatches(this ICommandCreator creator, string tableName, long maxId, int batchSize)
        {
            var total = 0;
            var stopWatch = new System.Diagnostics.Stopwatch();
            while (true)
            {
                stopWatch.Restart();
                var deleted = creator
                    .CreateSimple($"delete top ({batchSize}) from {tableName} with (readpast) where ID < @p0", maxId)
                    .ExecuteNonQuery();
                stopWatch.Stop();

                total += deleted;

                if (deleted > 0)
                    System.Threading.Tasks.Task.Delay((int)stopWatch.ElapsedMilliseconds).Wait();
                else
                    break;
            }
            return total;
        }

        public static TId InsertAndGetId<TVal, TId>(this ICommandCreator creator, string tableName, TVal value)
        {
            // Can be changed using CreateInsertAndGetID from Gerakul.FastSql.SqlServer.CommandCreatorExtensions
            return creator.CreateInsertWithOutput(tableName, value, new[] {"ID"}, "ID")
                .ExecuteQueryFirstColumn<TId>()
                .First();
        }

    }
}