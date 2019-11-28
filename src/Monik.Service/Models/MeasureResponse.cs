using System;

namespace Monik.Service
{
    public class MeasureResponse
    {
        public int MetricId { get; set; }
        public DateTime Interval { get; set; }
        public double Value { get; set; }
    }
}