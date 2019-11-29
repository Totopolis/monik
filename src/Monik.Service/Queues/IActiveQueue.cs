namespace Monik.Service
{
    public interface IActiveQueue
    {
        void Start(QueueReaderSettings config, ActiveQueueContext context);
        void Stop();
    }
}