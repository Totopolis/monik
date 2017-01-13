using Gerakul.FastSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monik;
using Monik.Common;
using System.Diagnostics;
using Microsoft.ServiceBus.Messaging;
using Monik.Client;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Hosting.Self;

namespace Monik.Service
{
  public class HelloModule : NancyModule
  {
    private IRepository FRepo;
    private ICacheLog FCacheLog;
    private ICacheKeepAlive FCacheKeepAlive;
    private IClientControl FControl;

    public HelloModule(IRepository aRepo, ICacheLog aCacheLog, ICacheKeepAlive aCacheKeepAlive, IClientControl aControl)
    {
      FRepo = aRepo;
      FCacheLog = aCacheLog;
      FCacheKeepAlive = aCacheKeepAlive;
      FControl = aControl;
      
      Get("/sources", args =>
      {
        try
        {
          List<Source> _result = FRepo.GetAllSources();
          return Response.AsJson<Source[]>(_result.ToArray());
        }
        catch(Exception _e)
        {
          FControl.ApplicationError($"Method /sources : {_e.Message}");
          return HttpStatusCode.InternalServerError;
        }
      });

      Get("/instances", args =>
      {
        try
        {
          List<Instance> _result = FRepo.GetAllInstances();
          return Response.AsJson<Instance[]>(_result.ToArray());
        }
        catch (Exception _e)
        {
          FControl.ApplicationError($"Method /instances : {_e.Message}");
          return HttpStatusCode.InternalServerError;
        }
      });

      Get("/groups", args =>
      {
        try
        {
          List<Group> _result = FRepo.GetAllGroupsAndFill();
          return Response.AsJson<Group[]>(_result.ToArray());
        }
        catch (Exception _e)
        {
          FControl.ApplicationError($"Method /instances : {_e.Message}");
          return HttpStatusCode.InternalServerError;
        }
      });

      Post("/logs3", args =>
      {
        int? _top = Request.Query["top"].HasValue ? Request.Query["top"] : null;
        string _order = Request.Query["order"].HasValue ? Request.Query["order"] : string.Empty;
        long? _lastid = Request.Query["lastid"].HasValue ? Request.Query["lastid"] : null;
        var _filters = this.Bind<LogsFilter[]>();

        try
        {
          List<Log_> _result = FCacheLog.GetLogs(_top, _order == "desc" ? Order.Desc : Order.Asc, _lastid, _filters);
          return Response.AsJson<Log_[]>(_result.ToArray());
        }
        catch (Exception _e)
        {
          FControl.ApplicationError($"Method /logs3 : {_e.Message}");
          return HttpStatusCode.InternalServerError;
        }
      });

      // only single and stream modes will be
      // get /logs4?group=main&top=100&lastid=123&severitycutoff=info&level=sec
      /* TODO: Get("/logs4", args =>
      {
        int? _group = Request.Query["group"].HasValue ? Request.Query["group"] : null;
        long? _lastid = Request.Query["lastid"].HasValue ? Request.Query["lastid"] : null;
        int? _severityCutoff = Request.Query["severitycutoff"].HasValue ? Request.Query["severitycutoff"] : null;
        int? _level = Request.Query["level"].HasValue ? Request.Query["level"] : null;
        int? _top = Request.Query["top"].HasValue ? Request.Query["top"] : null;

        try
        {
          List<Log_> _result = FCacheLog.GetLogs4(_group, _lastid, _severityCutoff, _level, _top);
          return Response.AsJson<Log_[]>(_result.ToArray());
        }
        catch (Exception _e)
        {
          FControl.ApplicationError($"Method /logs3 : {_e.Message}");
          return HttpStatusCode.InternalServerError;
        }
      });*/

      Post("/logs5", args =>
      {
        try
        {
          var _filter = this.Bind<LogRequest>();

          List<Log_> _result = FCacheLog.GetLogs5(_filter);
          return Response.AsJson<Log_[]>(_result.ToArray());
        }
        catch (Exception _e)
        {
          FControl.ApplicationError($"Method /logs5 : {_e.Message}");
          return HttpStatusCode.InternalServerError;
        }
      });

      Post("/keepalive", args =>
      {
        var _filters = this.Bind<LogsFilter[]>();

        try
        {
          List<KeepAlive_> _result = FCacheKeepAlive.GetKeepAlive(_filters);
          return Response.AsJson<KeepAlive_[]>(_result.ToArray());
        }
        catch (Exception _e)
        {
          FControl.ApplicationError($"Method /keepalive : {_e.Message}");
          return HttpStatusCode.InternalServerError;
        }
      });

      Post("/keepalive2", args =>
      {
        var _filter = this.Bind<KeepAliveRequest>();

        try
        {
          List<KeepAlive_> _result = FCacheKeepAlive.GetKeepAlive2(_filter);
          return Response.AsJson<KeepAlive_[]>(_result.ToArray());
        }
        catch (Exception _e)
        {
          FControl.ApplicationError($"Method /keepalive : {_e.Message}");
          return HttpStatusCode.InternalServerError;
        }
      });

      // TODO: /status
      // return: 
    }
  }

  public class WebService : IWebService
  {
    private string FPrefix;
    private NancyHost FWebServer;

    public WebService(string aPrefix)
    {
      FPrefix = aPrefix;
      FWebServer = new NancyHost(new Uri("http://" + aPrefix + "/")
        ,new Uri("http://localhost:2211/")
        );
    }

    public void OnStart()
    {
      try
      {
        FWebServer.Start();
        //M.ApplicationInfo($"Nancy web server started with prefix: {FPrefix}");
      }
      catch(Exception _e)
      {
        //M.ApplicationError($"Nancy web server start error: {_e.Message}");
      }
    }

    public void OnStop()
    {
      try
      {
        FWebServer.Stop();
      }
      catch (Exception _e)
      {
        //M.ApplicationError($"Nancy web server stop error: {_e.Message}");
      }
    }
  }//end of class
  
}
