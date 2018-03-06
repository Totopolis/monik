using System.Configuration;
using MonicWinService.Nancy;
using Monik.Client;
using Monik.Service;

namespace MonicWinService
{
    public class ServiceWorker : IObject
    {
        private WebService _service;
        private IClientControl _control;

        public void OnStart()
        {
            string prefix = ConfigurationManager.AppSettings["HTTP_EP"];

            _service = new WebService(prefix);
            _service.OnStart();

            _control = Bootstrapper.MainContainer.Resolve<IClientControl>();

            _control.ApplicationWarning("MonikWorker has been started");

        }

        public void OnStop()
        {
            _control.ApplicationWarning("MonikWorker is stopping");

            // TODO: catch exceptions inside

            _service.OnStop();

            var container = Bootstrapper.MainContainer;
            container.Resolve<IMessagePump>().OnStop();
            container.Resolve<IMessageProcessor>().OnStop();

            _control.ApplicationWarning("MonikWorker has stopped");

            _control.OnStop();


        }
    }
}