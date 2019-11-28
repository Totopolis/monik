using Monik.Common;

namespace Monik.Service
{
    public interface IMessageProcessor : IObject
    {
        void Process(Event ev, Instance instance);
        void FinalizeProcessing();
    }
}