using System.Collections.Concurrent;
using Monik.Common;

namespace Monik.Service
{
    public interface IMessagePump : IObject
    {
        void OnEmbeddedEvents(ConcurrentQueue<Event> events);
    }
}