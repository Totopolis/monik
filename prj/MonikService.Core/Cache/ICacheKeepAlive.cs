using System.Collections.Generic;
using MonikService.Core.Repository;
using MonikService.Core.Request;

namespace MonikService.Core.Cache
{
    public interface ICacheKeepAlive
    {
        long LastKeepAliveId { get; }
        void OnNewKeepAlive(KeepAlive_ aKeepAlive);

        List<KeepAlive_> GetKeepAlive2(KeepAliveRequest aFilter);
    }
}