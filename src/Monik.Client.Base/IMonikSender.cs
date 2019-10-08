using System.Collections.Concurrent;
using Monik.Common;

namespace Monik.Client
{
    public interface IMonikSender
    {
        void SendMessages(ConcurrentQueue<Event> aQueue);
    }
}
