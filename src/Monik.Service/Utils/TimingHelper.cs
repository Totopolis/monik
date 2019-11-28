using System;
using System.Runtime.CompilerServices;

namespace Monik.Common
{
    public class TimingHelper
    {
        private DateTime _from;
        private readonly IMonik _monik;

        private TimingHelper(IMonik aControl)
        {
            _from = DateTime.Now;
            _monik = aControl;
        }

        public static TimingHelper Create(IMonik monik)
        {
            return new TimingHelper(monik);
        }

        public void Begin()
        {
            _from = DateTime.Now;
        }

        public void EndAndLog([CallerMemberName] string sourceName = "")
        {
            var delta = DateTime.Now - _from;
            _monik.ApplicationInfo("{0} execution time: {1}ms", sourceName, delta.TotalMilliseconds);
        }

        public void EndAndMeasure(string metricName)
        {
            var delta = DateTime.Now - _from;
            _monik.Measure(metricName, AggregationType.Gauge, delta.TotalMilliseconds);
        }
    }
}