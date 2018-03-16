using System;
using System.Threading;

namespace MonikTestConsoleGenerator.Metrics
{
    public class Metric
    {
        public DateTime       InitTime      { get; set; }
        public MetricType     Type          { get; set; }
        public string         Name          { get; set; }
        public Action<Metric> Counter       { get; set; }
        public string         ServiceName   { get; set; }
        public string         SubSystemName { get; set; }

        private int currentValue;
        public  int CurrentValue                    => currentValue;
        public  int ExchangeCurrentValue(int value) => Interlocked.Exchange(ref currentValue, value);
        public  int AddToCurrentValue(int    value) => Interlocked.Add(ref currentValue, value);

        public Metric(string         serviceName,
                      string         subSystemName,
                      string         name,
                      DateTime       initTime,
                      MetricType     type,
                      Action<Metric> counter)
        {
            ServiceName   = serviceName;
            SubSystemName = subSystemName;
            Name          = name;
            InitTime      = initTime;
            Type          = type;
            Counter       = counter;
        }
    }
}