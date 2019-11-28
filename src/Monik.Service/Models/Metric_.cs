using System;

namespace Monik.Service
{
    public class Metric_
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int InstanceID { get; set; }
        public int Aggregation { get; set; }

        public long RangeHeadID { get; set; }
        public long RangeTailID { get; set; }

        public DateTime ActualInterval { get; set; }
        public long ActualID { get; set; }
    }
}