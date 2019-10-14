using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Monik.Common;

namespace Monik.Client
{
    public class MonikClient : MonikDelayedSender
    {
        private readonly IMonikSender _sender;

        private readonly CancellationTokenSource _keepAliveCancellationTokenSource;
        private readonly Task _keepAliveTask;

        public MonikClient(IMonikSender sender, IMonikSettings settings)
            : base(settings.SourceName, settings.InstanceName, settings.AutoKeepAliveInterval, settings.SendDelay)
        {
            _sender = sender;

            _keepAliveTask = null;
            _keepAliveCancellationTokenSource = null;

            if (settings.AutoKeepAliveEnable)
            {
                _keepAliveCancellationTokenSource = new CancellationTokenSource();
                _keepAliveTask = Task.Run(OnKeepAliveTask);
            }
        }

        public override void OnStop()
        {
            _keepAliveCancellationTokenSource?.Cancel();
            _keepAliveTask?.Wait();

            base.OnStop();
        }

        protected override Task OnSend(IEnumerable<Event> events)
        {
            return _sender.SendMessages(events);
        }

        private async Task OnKeepAliveTask()
        {
            while (!_keepAliveCancellationTokenSource.IsCancellationRequested)
            {
                KeepAlive();

                try
                {
                    await Task.Delay(_keepAliveInterval * 1000, _keepAliveCancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                }
            }
        }
    } //end of class
}