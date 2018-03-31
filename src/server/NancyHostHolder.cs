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
            _monik.ApplicationWarning("Started");

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
            try
            {
                _nancyHost.Stop();

                Bootstrapper.Global.Resolve<IMessagePump>().OnStop();
                Bootstrapper.Global.Resolve<IMessageProcessor>().OnStop();
            }
            catch (Exception e)
            {
                _monik.ApplicationError(e.Message);
            }

            _monik.ApplicationWarning("Stopped");
            _monik.OnStop();
        }
    }
}
