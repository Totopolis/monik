using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Monik.Client.Base;

namespace Monik.Common
{
    public abstract class MonikDelayedSender : MonikBase
    {
        private readonly Task _senderTask;
        private readonly ManualResetEventAsync _newMessageEvent = new ManualResetEventAsync(false, false);
        private readonly CancellationTokenSource _senderCancellationTokenSource = new CancellationTokenSource();

        private readonly ushort _sendDelay;
        private readonly int _waitTimeOnStop;

        private readonly ConcurrentQueue<Event> _msgQueue = new ConcurrentQueue<Event>();

        public MonikDelayedSender(string sourceName, string instanceName, ushort keepAliveInterval, ushort sendDelay, int waitTimeOnStop) :
            base(sourceName, instanceName, keepAliveInterval)
        {
            _waitTimeOnStop = waitTimeOnStop;
            _sendDelay = sendDelay;
            _senderTask = Task.Run(OnSenderTask);
        }

        public override void OnStop()
        {
            _senderCancellationTokenSource.Cancel();
            _senderTask.Wait(_waitTimeOnStop);
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

        private async Task OnSenderTask()
        {
            while (!_senderCancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    await _newMessageEvent.WaitAsync(_senderCancellationTokenSource.Token);
                    await Task.Delay(_sendDelay * 1000, _senderCancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // ignore
                }

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
                        await OnSend(messages);
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
