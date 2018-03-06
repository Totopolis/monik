using System.Collections.Generic;

namespace Monik.Service
{
    public interface ICacheLog : IObject
    {
        long LastLogId { get; }
        void OnNewLog(Log_ aLog);

        List<Log_> GetLogs5(LogRequest aFilter);
    }
}