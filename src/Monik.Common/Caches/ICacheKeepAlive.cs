using System.Collections.Generic;

namespace Monik.Service
{
    public interface ICacheKeepAlive : ICacheBase<KeepAlive_>
    {
        List<KeepAlive_> GetKeepAlive2(KeepAliveRequest filter);
    }
}