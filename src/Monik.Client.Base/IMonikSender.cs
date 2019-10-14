using System.Collections.Generic;
using System.Threading.Tasks;
using Monik.Common;

namespace Monik.Client
{
    public interface IMonikSender
    {
        Task SendMessages(IEnumerable<Event> events);
    }
}
