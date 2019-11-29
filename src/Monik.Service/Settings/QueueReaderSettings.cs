namespace Monik.Service
{
    public class QueueReaderSettings
    {
        public string Name { get; set; }
        public QueueReaderType Type { get; set; }
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
    }
}