namespace Monik.Service
{
    public enum QueueReaderType : byte
    {
        Azure = 1,
        RabbitMq = 2,
        SqlQueue = 3
    }
}