using System;

namespace Monik.Service
{
    public class KeepAlive_ : ICacheEntity
    {
        public long ID { get; set; }
        public DateTime Created { get; set; }
        public DateTime Received { get; set; }
        public int InstanceID { get; set; }
    }
}