using Monik.Common;

namespace MonikService.Core.Repository
{
    public interface IMessageProcessor : IObject
    {
        void Process(Event aEvent, Instance aInstance);
    }
}