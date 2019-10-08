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

        protected readonly ushort _sendDelay;

        private ConcurrentQueue<Event> _msgQueue = new ConcurrentQueue<Event>();

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

        private void FillMeasures(KeyValuePair<string, double>[] measures,
            AggregationType aggregation)
        {
            foreach (var measure in measures)
            {
                Event msg = NewEvent();

                msg.Mc = new Common.Metric()
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
                    if (_msgQueue.IsEmpty && _intermediateMeasures_Accum.IsEmpty)
                        continue;

                    var measures = _intermediateMeasures_Accum.ToArray();
                    _intermediateMeasures_Accum.Clear();

                    FillMeasures(measures, AggregationType.Accumulator);

                    measures = _intermediateMeasures_Gauge.ToArray();
                    _intermediateMeasures_Gauge.Clear();

                    FillMeasures(measures, AggregationType.Gauge);

                    OnSend(_msgQueue);
                }
                catch
                {
                    // TODO: ???  
                }
                finally
                {
                    _newMessageEvent.Reset();
                }
            }
        }

        protected abstract void OnSend(ConcurrentQueue<Event> events);

        protected override void OnNewMessage(Event msg)
        {
            if (msg != null)
                _msgQueue.Enqueue(msg);

            _newMessageEvent.Set();
        }
    }//end of class
}
