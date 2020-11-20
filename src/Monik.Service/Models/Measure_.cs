namespace Monik.Service
{
    public class Measure_
    {
        public long ID { get; set; }
        public double Value { get; set; }

        internal bool HasValue { get; set; }
    }

    public class MeasureWithMetricId : Measure_
    {
        public int MetricId { get; set; }
    }
}