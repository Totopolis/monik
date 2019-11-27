using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Monik.Service
{
    public abstract class RepositoryBase
    {
        protected const int MeasuresPerMetric = 4000;

        protected abstract IDbConnection Connection { get; }

        protected int ExecuteInBatches(string query, object param)
        {
            var total = 0;
            while (true)
            {
                var startTime = DateTime.UtcNow;
                int deleted;

                using (var con = Connection)
                {
                    deleted = con.Execute(query, param);
                    total += deleted;
                }

                if (deleted > 0)
                {
                    var toWait = Math.Max(1, (int) (DateTime.UtcNow - startTime).TotalMilliseconds);
                    Task.Delay(toWait).Wait();
                }
                else
                    break;
            }

            return total;
        }
    }
}