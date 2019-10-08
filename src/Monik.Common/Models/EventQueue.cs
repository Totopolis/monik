namespace Monik.Service
{
    public class EventQueue
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public EventQueueType Type { get; set; }
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
    }
}