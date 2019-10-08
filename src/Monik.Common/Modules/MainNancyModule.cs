using System;
using System.Collections.Generic;
using Monik.Common;
using Nancy;
using Nancy.ModelBinding;

namespace Monik.Service
{
    public sealed class MainNancyModule : NancyModule
    {
        private readonly IRepository _repo;
        private readonly ICacheLog _cacheLog;
        private readonly ICacheKeepAlive _cacheKeepAlive;
        private readonly ICacheMetric _cacheMetric;
        private readonly ICacheSourceInstance _sourceInstanceCache;
        private readonly IMonik _monik;

        public MainNancyModule(IRepository repo, ICacheLog cacheLog, ICacheKeepAlive cacheKeepAlive,
            ICacheMetric cacheMetric, ICacheSourceInstance sourceInstanceCache, IMonik monik)
        {
            _repo = repo;
            _cacheLog = cacheLog;
            _cacheKeepAlive = cacheKeepAlive;
            _cacheMetric = cacheMetric;
            _sourceInstanceCache = sourceInstanceCache;
            _monik = monik;

            Get("/sources", args =>
            {
                try
                {
                    List<Source> result = sourceInstanceCache.GetAllSources();
                    return Response.AsJson<Source[]>(result.ToArray());
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method /sources : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Get("/instances", args =>
            {
                try
                {
                    List<Instance> result = sourceInstanceCache.GetAllInstances();
                    return Response.AsJson<Instance[]>(result.ToArray());
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method /instances : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Get("/groups", args =>
            {
                try
                {
                    List<Group> result = sourceInstanceCache.GetAllGroups();
                    return Response.AsJson<Group[]>(result.ToArray());
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method /groups : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Post("/logs5", args =>
            {
                try
                {
                    var filter = this.Bind<LogRequest>();

                    List<Log_> result = cacheLog.GetLogs5(filter);
                    return Response.AsJson<Log_[]>(result.ToArray());
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method /logs5 : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Post("/keepalive2", args =>
            {
                var filter = this.Bind<KeepAliveRequest>();

                try
                {
                    List<KeepAlive_> result = cacheKeepAlive.GetKeepAlive2(filter);
                    return Response.AsJson<KeepAlive_[]>(result.ToArray());
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method /keepalive : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Get("/keepalive-status", args =>
            {
                var filter = new KeepAliveRequest();
                return GetKeepAliveStatuses(filter);
            });

            Post("/keepalive-status", args =>
            {
                var filter = this.Bind<KeepAliveRequest>();
                return GetKeepAliveStatuses(filter);
            });

            Get("/metrics", args =>
            {
                try
                {
                    var result = cacheMetric.GetMetricsDescriptions();
                    return Response.AsJson(result);
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method /metrics : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Get("/metrics/currents", args =>
            {
                try
                {
                    var result = cacheMetric.GetAllCurrentMeasures();
                    return Response.AsJson(result);
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method /metrics/currents : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Post("/metrics/currents", args =>
            {
                try
                {
                    var filter = this.Bind<MetricRequest>();

                    var result = cacheMetric.GetCurrentMeasures(filter);
                    return Response.AsJson(result);
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method POST /metrics/currents : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Get("/metrics/{id:int}/current", args =>
            {
                try
                {
                    int metricId = args.id;

                    var result = cacheMetric.GetCurrentMeasure(metricId);
                    return Response.AsJson(result);
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method /metrics/id/current : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Get("/metrics/windows", args =>
            {
                try
                {
                    var result = cacheMetric.GetAllWindowsMeasures();
                    return Response.AsJson(result);
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method /metrics/windows : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Post("/metrics/windows", args =>
            {
                try
                {
                    var filter = this.Bind<MetricRequest>();

                    var result = cacheMetric.GetWindowMeasures(filter);
                    return Response.AsJson(result);
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method POST /metrics/windows : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Get("/metrics/{id:int}/window", args =>
            {
                try
                {
                    int metricId = args.id;

                    var result = cacheMetric.GetWindowMeasure(metricId);
                    return Response.AsJson(result);
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method /metrics/id/window : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Get("/metrics/{id:int}/history", args =>
            {
                try
                {
                    int metricId = args.id;
                    var p = this.Bind<MetricHistoryRequestParameters>();
                    var result = cacheMetric.GetMetricHistory(metricId, p.Amount, p.Skip);
                    return Response.AsJson(result);
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method /metrics/id/history : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });
        }


        private dynamic GetKeepAliveStatuses(KeepAliveRequest filter)
        {
            try
            {
                var kaResult = _cacheKeepAlive.GetKeepAlive2(filter);
                var result = new List<KeepAliveStatus>();

                foreach (var ka in kaResult)
                {
                    var inst = _sourceInstanceCache.GetInstanceById(ka.InstanceID);

                    KeepAliveStatus status = new KeepAliveStatus()
                    {
                        SourceID = inst.SourceID,
                        InstanceID = inst.ID,
                        SourceName = inst.SourceRef().Name,
                        InstanceName = inst.Name,
                        DisplayName = inst.SourceRef().Name + "." + inst.Name,
                        Created = ka.Created,
                        Received = ka.Received,
                        StatusOK = (DateTime.UtcNow - ka.Created).TotalSeconds < 180 // in seconds
                                                                                     // TODO: use param or default value for delta seconds
                    };

                    result.Add(status);
                }

                return Response.AsJson(result.ToArray());
            }
            catch (Exception ex)
            {
                _monik.ApplicationError($"Method /status : {ex.Message}");
                return HttpStatusCode.InternalServerError;
            }
        }

    }//end of class

    //end of class

}
