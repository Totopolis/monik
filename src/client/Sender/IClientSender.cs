using System.Collections.Concurrent;
using Monik.Common;

namespace Monik.Client.Sender
{
    public interface IClientSender
    {
        void SendMessages(ConcurrentQueue<Event> aQueue);
    }
}