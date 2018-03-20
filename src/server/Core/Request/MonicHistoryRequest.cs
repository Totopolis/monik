using System;

namespace MonikService.Core.Request
{
    public class MonicHistoryRequest
    {
        public long MetricId { get; set; }
        public int Count { get; set; }
        public DateTime? LastWindowCreated { get; set; }
    }
}