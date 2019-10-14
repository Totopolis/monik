using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Monik.Common
{
    public abstract class MonikDelayedSender : MonikBase
    {
        private const int SenderTaskWaitingTimeOnStop = 10_000;

        private readonly Task _senderTask;
        private readonly ManualResetEvent _newMessageEvent = new ManualResetEvent(false);
        private readonly CancellationTokenSource _senderCancellationTokenSource = new CancellationTokenSource();

        private readonly ushort _sendDelay;

        private readonly ConcurrentQueue<Event> _msgQueue = new ConcurrentQueue<Event>();

        public MonikDelayedSender(string sourceName, string instanceName, ushort keepAliveInterval, ushort sendDelay) :
            base(sourceName, instanceName, keepAliveInterval)
        {
            _sendDelay = sendDelay;
            _senderTask = Task.Run(OnSenderTask);
        }

        public override void OnStop()
        {
            _newMessageEvent.Set();
            _senderCancellationTokenSource.Cancel();

            _senderTask.Wait(SenderTaskWaitingTimeOnStop);
        }

        // TODO: MAX/MIN aggregation type ?

        private void FillMeasures(IEnumerable<KeyValuePair<string, double>> measures,
            AggregationType aggregation)
        {
            foreach (var measure in measures)
            {
                Event msg = NewEvent();

                msg.Mc = new Metric
                {
                    Name = measure.Key,
                    Aggregation = aggregation,
                    Value = measure.Value
                };

                _msgQueue.Enqueue(msg);
            }//for
        }

        private void OnSenderTask()
        {
            while (!_senderCancellationTokenSource.IsCancellationRequested)
            {
                _newMessageEvent.WaitOne();

                int msDelay = _sendDelay * 1000;
                Task.Delay(msDelay).Wait();

                try
                {
                    if (!IntermediateMeasuresAccum.IsEmpty)
                    {
                        var measures = IntermediateMeasuresAccum;
                        IntermediateMeasuresAccum = new ConcurrentDictionary<string, double>();
                        FillMeasures(measures, AggregationType.Accumulator);
                    }

                    if (!IntermediateMeasuresGauge.IsEmpty)
                    {
                        var measures = IntermediateMeasuresGauge;
                        IntermediateMeasuresGauge = new ConcurrentDictionary<string, double>();
                        FillMeasures(measures, AggregationType.Gauge);
                    }

                    if (_msgQueue.TryDequeueAll(out var messages))
                        OnSend(messages).Wait();
                }
                catch
                {
                    // ignore
                }
                finally
                {
                    _newMessageEvent.Reset();
                }
            }
        }

        protected abstract Task OnSend(IEnumerable<Event> events);

        protected override void OnNewMessage(Event msg)
        {
            if (msg != null)
                _msgQueue.Enqueue(msg);

            _newMessageEvent.Set();
        }
    }//end of class
}
