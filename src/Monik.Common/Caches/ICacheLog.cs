using System.Collections.Generic;

namespace Monik.Service
{
    public interface ICacheLog : ICacheBase<Log_>
    {
        List<Log_> GetLogs5(LogRequest filter);
    }
}