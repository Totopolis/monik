using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using Monik.Common;

namespace Monik.Client
{
    public class MonikInstance : MonikInstanceBase
    {
        private readonly IMonikSender _sender;

        private Task FSenderTask;
        private readonly ManualResetEvent FNewMessageEvent = new ManualResetEvent(false);
        private readonly CancellationTokenSource FSenderCancellationTokenSource = new CancellationTokenSource();

        private ushort FSendDelay;

        private CancellationTokenSource FAutoKeepAliveCancellationTokenSource;

        private Task FAutoKeepAliveTask;

        private bool FAutoKeepAliveEnable;

        private bool AutoKeepAliveEnable
        {
            get { return FAutoKeepAliveEnable; }
            set
            {
                if (FAutoKeepAliveEnable == value)
                    return;

                FAutoKeepAliveEnable = value;

                if (FAutoKeepAliveEnable == false)
                    FAutoKeepAliveCancellationTokenSource.Cancel();
                else
                {
                    FAutoKeepAliveCancellationTokenSource = new CancellationTokenSource();
                    FAutoKeepAliveTask = Task.Run(() => { OnAutoKeepAliveTask(); });
                }
            }
        }

        private void OnAutoKeepAliveTask()
        {
            while (!FAutoKeepAliveCancellationTokenSource.IsCancellationRequested)
            {
                int msDelay = _keepAliveInterval * 1000;
                Task.Delay(msDelay).Wait();

                KeepAlive();
            }
        }

        public MonikInstance(IMonikSender sender, IMonikSettings settings)
            : base(settings.SourceName, settings.InstanceName, settings.AutoKeepAliveInterval)
        {
            _sender = sender;

            // TODO: when IDisposable from subscribe will be raise?

            FSendDelay = settings.SendDelay;

            FAutoKeepAliveEnable = false;
            FAutoKeepAliveTask = null;
            FAutoKeepAliveCancellationTokenSource = null;

            AutoKeepAliveEnable = settings.AutoKeepAliveEnable;

            FSenderTask = Task.Run(() => { OnSenderTask(); });
        }

        public override void OnStop()
        {
            // TODO: is it correct?
            FNewMessageEvent.Set();
            FSenderCancellationTokenSource.Cancel();

            if (FAutoKeepAliveEnable)
                FAutoKeepAliveCancellationTokenSource.Cancel();

            // TODO: may be mor efficient?
            Task.Delay(2000).Wait();
        }

        private void OnSenderTask()
        {
            while (!FSenderCancellationTokenSource.IsCancellationRequested)
            {
                FNewMessageEvent.WaitOne();

                int msDelay = FSendDelay * 1000;
                Task.Delay(msDelay).Wait();

                try
                {
                    if (FMsgQueue.IsEmpty)
                        continue;

                    _sender.SendMessages(FMsgQueue);
                }
                catch
                {
                    // TODO: ???  
                }
                finally
                {
                    FNewMessageEvent.Reset();
                }
            }
        }

        protected override void PushLogToSend(string aBody, LevelType aLevel, SeverityType aSeverity, params object[] aParams)
        {
            base.PushLogToSend(aBody, aLevel, aSeverity, aParams);

            FNewMessageEvent.Set();
        }

        public override void KeepAlive()
        {
            base.KeepAlive();

            FNewMessageEvent.Set();
        }
    }//end of class
}
