using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using Monik.Service;
using Monik.Common;
using Autofac;

namespace MonikWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent _runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            _monik.ApplicationVerbose("MonikWorker is running");

            try
            {
                this.RunAsync(this._cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this._runCompleteEvent.Set();
            }
        }

        private NancyHostHolder _nancyHost;
        private IMonik _monik;

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;
            bool result = base.OnStart();

            // TODO: retry logic and exit if exceptions in Bootstrapper...

            //string prefix = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["HTTP_EP"].IPEndpoint.ToString();

            // TODO : port mapping

            _nancyHost = new NancyHostHolder();
            _nancyHost.Start();

            _monik = Bootstrapper.Global.Resolve<IMonik>();
            _monik.ApplicationWarning("MonikWorker has been started");

            return result;
        }

        public override void OnStop()
        {
            _monik.ApplicationWarning("MonikWorker is stopping");

            // TODO: catch exceptions inside

            _nancyHost.Stop();

            this._cancellationTokenSource.Cancel();
            this._runCompleteEvent.WaitOne();

            // TODO: Pump already stopped !!! msg will be lost !!!
            // _monik.ApplicationWarning("MonikWorker has stopped");

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
