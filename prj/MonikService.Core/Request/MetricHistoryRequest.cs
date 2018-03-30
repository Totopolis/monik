using System;

namespace MonikService.Core.Request
{
    public class MetricHistoryRequest
    {
        public long      MetricId          { get; set; }
        public int       Count             { get; set; }
        public DateTime? LastWindowCreated { get; set; }
    }
}