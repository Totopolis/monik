using System;
using System.Collections.Generic;
using Monik.Client.ClientControl;
using MonikService.Core;
using MonikService.Core.Cache;
using MonikService.Core.Request;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Hosting.Self;

namespace Monik.Service
{
    public class HelloModule : NancyModule
    {
        public HelloModule(ICacheLog            aCacheLog,
                           ICacheKeepAlive      aCacheKeepAlive,
                           ISourceInstanceCache aSourceInstanceCache,
                           IClientControl       aControl,
                           ICacheMetrics        aCacheMetrics)
        {
            var cacheLog            = aCacheLog;
            var cacheKeepAlive      = aCacheKeepAlive;
            var cacheSourceInstance = aSourceInstanceCache;
            var cacheMetrics        = aCacheMetrics;
            var control             = aControl;

            Get("/sources", args =>
            {
                try
                {
                    var result = cacheSourceInstance.GetAllSources();
                    return Response.AsJson(result);
                }
                catch (Exception ex)
                {
                    control.ApplicationError($"Method /sources : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            // TODO: use cacheSourceInstance
            Get("/instances", args =>
            {
                try
                {
                    var result = cacheSourceInstance.GetAllInstances();
                    return Response.AsJson(result);
                }
                catch (Exception ex)
                {
                    control.ApplicationError($"Method /instances : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Get("/groups", args =>
            {
                try
                {
                    var result = cacheSourceInstance.GetAllGroups();
                    return Response.AsJson(result);
                }
                catch (Exception ex)
                {
                    control.ApplicationError($"Method /instances : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Post("/logs5", args =>
            {
                try
                {
                    var filter = this.Bind<LogRequest>();

                    var result = cacheLog.GetLogs5(filter);
                    return Response.AsJson(result);
                }
                catch (Exception ex)
                {
                    control.ApplicationError($"Method /logs5 : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Post("/keepalive2", args =>
            {
                var filter = this.Bind<KeepAliveRequest>();

                try
                {
                    var result = cacheKeepAlive.GetKeepAlive2(filter);
                    return Response.AsJson(result);
                }
                catch (Exception ex)
                {
                    control.ApplicationError($"Method /keepalive : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Get("/keepalive-status", args =>
            {
                try
                {
                    var filter   = new KeepAliveRequest();
                    var kaResult = cacheKeepAlive.GetKeepAlive2(filter);
                    var result   = new List<KeepAliveStatus>();

                    foreach (var ka in kaResult)
                    {
                        var inst = cacheSourceInstance.GetInstanceById(ka.InstanceID);

                        KeepAliveStatus status = new KeepAliveStatus()
                        {
                            SourceID     = inst.SourceID,
                            InstanceID   = inst.ID,
                            SourceName   = inst.SourceRef().Name,
                            InstanceName = inst.Name,
                            DisplayName  = inst.SourceRef().Name + "." + inst.Name,
                            Created      = ka.Created,
                            Received     = ka.Received,
                            StatusOK     = (DateTime.UtcNow - ka.Created).TotalSeconds < 120 // in seconds
                            // TODO: use param or default value for delta seconds
                        };

                        result.Add(status);
                    }

                    return Response.AsJson(result);
                }
                catch (Exception ex)
                {
                    control.ApplicationError($"Method /status : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Get("/metricDescriptions", args =>
            {
                try
                {
                    var result = cacheMetrics.GetMetricDescriptions();
                    return Response.AsJson(result);
                }
                catch (Exception ex)
                {
                    control.ApplicationError($"Method /metricDescriptions : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Get("/metrics", args =>
            {
                try
                {
                    var result = cacheMetrics.GetAllCurrentMetricValues();
                    return Response.AsJson(result);
                }
                catch (Exception ex)
                {
                    control.ApplicationError($"Method /metrics : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });
        }
    }

    public class WebService : IObject
    {
        private          string    _prefix;
        private readonly NancyHost _webServer;

        public WebService(string aPrefix)
        {
            _prefix = aPrefix;

            var hostConfiguration =
                new HostConfiguration() {UrlReservations = new UrlReservations() {CreateAutomatically = true}};

            _webServer = new NancyHost(hostConfiguration,
                                       new Uri("http://" + aPrefix + "/"),
                                       new Uri("http://localhost:2211/"));
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