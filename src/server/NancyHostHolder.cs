using System;
using Autofac;
using Monik.Common;
using Nancy.Hosting.Self;

namespace Monik.Service
{
    public class NancyHostHolder
    {
        private readonly IMonik _monik;
        private readonly NancyHost _nancyHost;

        public NancyHostHolder()
        {
            _nancyHost = new NancyHost(
                new Uri("http://localhost:2211"), 
                new Bootstrapper(), 
                HostConfigs);

            _monik = Bootstrapper.Global.Resolve<IMonik>();
            _monik.ApplicationInfo("HostHolder.ctor");
        }

        private static readonly HostConfiguration HostConfigs = new HostConfiguration()
        {
            UrlReservations = new UrlReservations() {CreateAutomatically = true}, RewriteLocalhost = true
        };

        public void Start()
        {
            _monik.ApplicationWarning("HostHolder starting");

            try
            {
                _nancyHost.Start();
            }
            catch (Exception e)
            {
                _monik.ApplicationError(e.Message);
            }
        }

        public void Stop()
        {
            _monik.ApplicationWarning("HostHolder stopping");

            try
            {
                _nancyHost.Stop();

                Bootstrapper.Global.Resolve<IMonik>().OnStop();
                Bootstrapper.Global.Resolve<IMessagePump>().OnStop();
                Bootstrapper.Global.Resolve<IMessageProcessor>().OnStop();
            }
            catch (Exception e)
            {
                _monik.ApplicationError(e.Message);
            }

            // TODO: pump already stopped ! msg will be lost !!!
            //_monik.ApplicationWarning("Stopped");
            //_monik.OnStop();
        }
    }
}
