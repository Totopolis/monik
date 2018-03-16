using System.Collections.Generic;
using MonikService.Core.Repository;

namespace MonikService.Core.Cache
{
    public interface ICacheMetrics
    {
        void                    AddMetricAggregatingValue(MetricDescription metricDesc, MetricValue value);
        void                    OnStop();
        List<MetricDescription> GetMetricDescriptions();
        List<MetricValue>       GetAllCurrentMetricValues();
    }
}