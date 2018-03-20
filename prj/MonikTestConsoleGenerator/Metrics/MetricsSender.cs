using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MonikTestConsoleGenerator.Metrics
{
    public class MetricsSender
    {
        public Dictionary<string, Metric> Metrics { get; set; }

        public void AddMetric(Metric metric)
        {
            //Metrics.Add(metric.ServiceName+"."+metric.SubSystemName+"."+metric.Name, metric);
            Metrics.Add(metric.Name, metric);
        }

        public MonikTestGeneratorInstance MonikTestGeneratorInstance { get; }
        public TimeSpan                   MetricSendingDelay         { get; }

        public MetricsSender(MonikTestGeneratorInstance monikTestGeneratorInstance, TimeSpan metricSendingDelay)
        {
            MonikTestGeneratorInstance = monikTestGeneratorInstance;
            MetricSendingDelay         = metricSendingDelay;

            var InitTime = DateTime.Now;
            Metrics = new Dictionary<string, Metric>();

            AddMetric(new Metric("FakeMetricsService", "", "SinHourGauge0_5", InitTime, MetricType.Gauge, (metric) =>
            {
                var rad = (DateTime.Now - metric.InitTime).TotalHours * 2 * Math.PI;
                metric.ExchangeCurrentValue((int) ((Math.Sin(rad) + 1) * 3));
            }));

            AddMetric(new Metric("FakeMetricsService", "", "CosHourGauge0_10", InitTime, MetricType.Gauge, (metric) =>
            {
                var rad = (DateTime.Now - metric.InitTime).TotalHours * 2 * Math.PI;
                metric.ExchangeCurrentValue((int) ((Math.Cos(rad) + 1) * 5.5));
            }));

            AddMetric(new Metric("FakeMetricsService", "", "SinHourAccum0_100", InitTime.AddMinutes(-15), MetricType.Accum, (metric) =>
            {
                var rad = (DateTime.Now - metric.InitTime).TotalHours * 2 * Math.PI;
                metric.AddToCurrentValue((int) ((Math.Cos(rad) + 1) * 50.5 * MetricSendingDelay.TotalSeconds / (5 * 60)));
            }));
        }

        private CancellationTokenSource cancelTokenSource { get; } = new CancellationTokenSource();

        public void Stop()
        {
            cancelTokenSource?.Cancel();
        }

        public void StartSendingMetrics()
        {
            CancellationToken token = cancelTokenSource.Token;

            Task.Run(() => Process(token), token).Wait(token);
        }

        private void Process(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                foreach (var metric in Metrics.Values)
                {
                    metric.Counter(metric);

                    if (metric.Type == MetricType.Accum)
                    {
                        var metricVal = metric.ExchangeCurrentValue(0);

                        MonikTestGeneratorInstance.Metric(
                            metric.Name,
                            metricVal,
                            Monik.Common.MetricType.Accumulator);
                    }
                    else
                    {
                        MonikTestGeneratorInstance.Metric(
                            metric.Name,
                            metric.CurrentValue,
                            Monik.Common.MetricType.Gauge);
                    }
                }

                Task.Delay(MetricSendingDelay, token).Wait(token);
            }
        }
    }
}