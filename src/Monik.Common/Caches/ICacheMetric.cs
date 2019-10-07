using Monik.Common;

namespace Monik.Service
{
    public interface ICacheMetric : IObject
    {
        void OnNewMeasure(Instance instance, Event metric);

        void RemoveMetric(int id);

        Metric_[] GetMetricsDescriptions();
        MeasureResponse GetCurrentMeasure(int metricId);
        MeasureResponse[] GetCurrentMeasures(MetricRequest filter);
        MeasureResponse[] GetAllCurrentMeasures();
        WindowResponse[] GetWindowMeasures(MetricRequest filter);
        WindowResponse[] GetAllWindowsMeasures();
        WindowResponse GetWindowMeasure(int metricId);
        MetricHistoryResponse GetMetricHistory(int metricId, int amount, int skip);
    }
}