using System;
using System.Configuration;
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
                new Uri(ConfigurationManager.AppSettings["Url"]), 
                new Bootstrapper(), 
                HostConfigs);

            _monik = Bootstrapper.Singleton.Resolve<IMonik>();
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

                Bootstrapper.Singleton.OnApplicationStop();
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
