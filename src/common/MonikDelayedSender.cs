using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Monik.Common
{
    public abstract class MonikDelayedSender : MonikBase
    {
        private readonly Task _senderTask;
        private readonly ManualResetEvent _newMessageEvent = new ManualResetEvent(false);
        private readonly CancellationTokenSource _senderCancellationTokenSource = new CancellationTokenSource();

        protected readonly ushort _sendDelay;

        private ConcurrentQueue<Event> _msgQueue = new ConcurrentQueue<Event>();

        public MonikDelayedSender(string sourceName, string instanceName, ushort keepAliveInterval, ushort sendDelay) :
            base(sourceName, instanceName, keepAliveInterval)
        {
            _sendDelay = sendDelay;
            _senderTask = Task.Run(() => { OnSenderTask(); });
        }

        public override void OnStop()
        {
            // TODO: is it correct?
            _newMessageEvent.Set();
            _senderCancellationTokenSource.Cancel();

            Task.Delay(2000).Wait();
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
                    if (_msgQueue.IsEmpty)
                        continue;

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
            _msgQueue.Enqueue(msg);
            _newMessageEvent.Set();
        }
    }//end of class
}
