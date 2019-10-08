using System;

namespace Monik.Service
{
    public class Log_ : ICacheEntity
    {
        public long ID { get; set; }
        public DateTime Created { get; set; }
        public DateTime Received { get; set; }
        public byte Level { get; set; }
        public byte Severity { get; set; }
        public int InstanceID { get; set; }
        public byte Format { get; set; }
        public string Body { get; set; }
        public string Tags { get; set; }
    }
}