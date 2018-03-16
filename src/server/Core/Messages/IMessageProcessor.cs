using Monik.Common;
using MonikService.Core.Repository;

namespace MonikService.Core.Messages
{
    public interface IMessageProcessor : IObject
    {
        void Process(Event aEvent, Instance aInstance);
    }
}