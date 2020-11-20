using Monik.Common;

namespace Monik.Service
{
    public interface IMetricObject : IObject
    {
        Metric_ Dto { get; }

        MeasureResponse GetCurrentMeasure();
        WindowResponse GetWindow();
        MetricHistoryResponse GetMetricHistory(int amount, int skip);

        void CreateNew(string name, int aggregation, Instance instance);
        void Load(Metric_ metric, Measure_[] measures);

        void OnNewMeasure(Event metric);

        void BackgroundIntervalPush();
        void BackgroundSecondPush();
    }
}