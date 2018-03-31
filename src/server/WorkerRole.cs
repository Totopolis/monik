using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using Monik.Service;
using Monik.Client;

namespace MonikWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent _runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            _control.ApplicationVerbose("MonikWorker is running");

            try
            {
                this.RunAsync(this._cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this._runCompleteEvent.Set();
            }
        }

        private WebService _service;
        private IClientControl _control;

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;
            bool result = base.OnStart();

            // TODO: retry logic and exit if exceptions in Bootstrapper...

            string prefix = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["HTTP_EP"].IPEndpoint.ToString();
            _service = new WebService(prefix);
            _service.OnStart();

            _control = Bootstrapper.MainContainer.Resolve<IClientControl>();

            _control.ApplicationWarning("MonikWorker has been started");

            return result;
        }

        public override void OnStop()
        {
            _control.ApplicationWarning("MonikWorker is stopping");

            // TODO: catch exceptions inside

            _service.OnStop();

            var container = Bootstrapper.MainContainer;
            container.Resolve<IMessagePump>().OnStop();
            container.Resolve<IMessageProcessor>().OnStop();

            this._cancellationTokenSource.Cancel();
            this._runCompleteEvent.WaitOne();

            _control.ApplicationWarning("MonikWorker has stopped");

            _control.OnStop();

            base.OnStop();
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                //M.ApplicationInfo("Working");
                await Task.Delay(10000);
            }
        }
    }
}
