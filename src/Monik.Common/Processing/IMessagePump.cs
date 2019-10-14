using System.Collections.Generic;
using Monik.Common;

namespace Monik.Service
{
    public interface IMessagePump : IObject
    {
        void OnEmbeddedEvents(IEnumerable<Event> events);
    }
}