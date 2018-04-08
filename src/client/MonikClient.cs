using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using Monik.Common;

namespace Monik.Client
{
    public class MonikClient : MonikDelayedSender
    {
        private readonly IMonikSender _sender;

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

        public MonikClient(IMonikSender sender, IMonikSettings settings)
            : base(settings.SourceName, settings.InstanceName, settings.AutoKeepAliveInterval, settings.SendDelay)
        {
            _sender = sender;

            // TODO: when IDisposable from subscribe will be raise?

            FAutoKeepAliveEnable = false;
            FAutoKeepAliveTask = null;
            FAutoKeepAliveCancellationTokenSource = null;

            AutoKeepAliveEnable = settings.AutoKeepAliveEnable;
        }

        public override void OnStop()
        {
            if (FAutoKeepAliveEnable)
                FAutoKeepAliveCancellationTokenSource.Cancel();

            base.OnStop();
        }

        protected override void OnSend(ConcurrentQueue<Event> events)
        {
            _sender.SendMessages(events);
        }
    }//end of class
}
