using System.Collections.Generic;

namespace Monik.Service
{
    public interface ICacheKeepAlive : IObject
    {
        long LastKeepAliveId { get; }
        void OnNewKeepAlive(KeepAlive_ aKeepAlive);

        List<KeepAlive_> GetKeepAlive2(KeepAliveRequest aFilter);
    }
}