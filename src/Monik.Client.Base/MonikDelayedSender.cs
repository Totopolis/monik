using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
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
        private readonly bool _groupDuplicates;

        private readonly Channel<Event> _msgQueue;

        public MonikDelayedSender(string sourceName, string instanceName,
            ushort keepAliveInterval, ushort sendDelay, int waitTimeOnStop,
            bool groupDuplicates, int queueCapacity) :
            base(sourceName, instanceName, keepAliveInterval)
        {
            _msgQueue = queueCapacity < 1
                ? Channel.CreateUnbounded<Event>(new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false
                })
                : Channel.CreateBounded<Event>(new BoundedChannelOptions(queueCapacity)
                {
                    FullMode = BoundedChannelFullMode.DropOldest,
                    SingleReader = true,
                    SingleWriter = false
                });
            _groupDuplicates = groupDuplicates;
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

                _msgQueue.Writer.TryWrite(msg);
            } //for
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

                    if (_msgQueue.Reader.TryReadAll(out var messages))
                        await SendMessages(messages);
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
                _msgQueue.Writer.TryWrite(msg);

            _newMessageEvent.Set();
        }


        private async Task SendMessages(IList<Event> messages)
        {
            try
            {
                if (_groupDuplicates)
                    messages = messages.GroupDuplicates();

                await OnSend(messages);
            }
            catch
            {
                // ignored
            }
        }
    } //end of class
}