using System;

namespace Monik.Service
{
    public class KeepAliveStatus
    {
        public short SourceID { get; set; }
        public int InstanceID { get; set; }

        public string SourceName { get; set; }
        public string InstanceName { get; set; }
        public string DisplayName { get; set; }

        public DateTime Created { get; set; }
        public DateTime Received { get; set; }

        public bool StatusOK { get; set; }
    }
}