using System;

namespace Monik.Service
{
    public class MetricHistoryResponse
    {
        public int MetricId { get; set; }
        public DateTime Interval { get; set; }
        public double[] Values { get; set; }
    }
}