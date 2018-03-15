using System.Collections.Generic;

namespace Monik.Service
{
    public interface ICacheMetrics
    {
        void AddMetricAggregatingValue(MetricDescription metricDesc, MetricValue value);
        void OnStop();
    }
}