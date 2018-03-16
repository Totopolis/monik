using System.Collections.Generic;
using MonikService.Core.Repository;
using MonikService.Core.Request;

namespace MonikService.Core.Cache
{
    public interface ICacheLog : IObject
    {
        long LastLogId { get; }
        void OnNewLog(Log_ aLog);

        List<Log_> GetLogs5(LogRequest aFilter);
    }
}