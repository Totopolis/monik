using System;
using System.Collections.Generic;
using Monik.Common;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Hosting.Self;

namespace Monik.Service
{
    public class WebService : IWebService
    {
        private string _prefix;
        private readonly NancyHost _webServer;

        public WebService(string aPrefix)
        {
            _prefix = aPrefix;
            _webServer = new NancyHost(new Uri("http://" + aPrefix + "/")
                , new Uri("http://localhost:2211/")
            );
        }

        public void OnStart()
        {
            try
            {
                _webServer.Start();
                //M.ApplicationInfo($"Nancy web server started with prefix: {_prefix}");
            }
            catch (Exception)
            {
                //M.ApplicationError($"Nancy web server start error: {_e.Message}");
            }
        }

        public void OnStop()
        {
            try
            {
                _webServer.Stop();
            }
            catch (Exception)
            {
                //M.ApplicationError($"Nancy web server stop error: {_e.Message}");
            }
        }

    } //end of class
}
