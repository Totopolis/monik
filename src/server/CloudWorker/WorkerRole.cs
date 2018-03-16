using System.Net;
using Microsoft.WindowsAzure.ServiceRuntime;
using Monik.Service;
using Monik.Client.ClientControl;
using MonikService.Core.Messages;
using MonikWorker.Nancy;

namespace MonikWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            _control.ApplicationVerbose("MonikWorker is running");
            base.Run();
        }

        private WebService     _service;
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
            
            _control.ApplicationWarning("MonikWorker has stopped");

            _control.OnStop();

            base.OnStop();
        }
    }
}