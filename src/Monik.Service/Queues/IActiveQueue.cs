namespace Monik.Service
{
    public interface IActiveQueue
    {
        void Start(EventQueue config, ActiveQueueContext context);
        void Stop();
    }
}