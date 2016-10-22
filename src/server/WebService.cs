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

    public HelloModule(IRepository aRepo)
    {
      FRepo = aRepo;
      
      Get("/sources", args =>
      {
        try
        {
          List<WebServiceSourcesResponse> _result = FRepo.GetSources();
          return Response.AsJson<WebServiceSourcesResponse[]>(_result.ToArray());
        }
        catch { return HttpStatusCode.InternalServerError; }
      });

      Get("/instances", args =>
      {
        try
        {
          List<WebServiceInstancesResponse> _result = FRepo.GetInstances();
          return Response.AsJson<WebServiceInstancesResponse[]>(_result.ToArray());
        }
        catch { return HttpStatusCode.InternalServerError; }
      });

      Post("/logs", args =>
      {
        var _top = Request.Query["top"];
        var _filters = this.Bind<LogsFilter[]>();

        try
        {
          List<WebServiceLogResponse> _result = FRepo.GetFilteredLogs(_top, _filters);
          return Response.AsJson<WebServiceLogResponse[]>(_result.ToArray());
        }
        catch { return HttpStatusCode.InternalServerError; }
      });

      Post("/logs2", args =>
      {
        int? _top = Request.Query["top"].HasValue ? Request.Query["top"] : null;
        string _order = Request.Query["order"].HasValue ? Request.Query["order"] : string.Empty;
        long? _lastid = Request.Query["lastid"].HasValue ? Request.Query["lastid"] : null;
        var _filters = this.Bind<LogsFilter[]>();

        try
        {
          List<WebServiceLogResponse> _result = FRepo.GetFilteredLogs2(_top, _order, _lastid, _filters);
          return Response.AsJson<WebServiceLogResponse[]>(_result.ToArray());
        }
        catch { return HttpStatusCode.InternalServerError; }
      });
    }
  }

  public class WebService
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
        M.ApplicationInfo("Nancy web server started with prefix: " + FPrefix);
      }
      catch(Exception _e)
      {
        M.ApplicationError("Nancy web server start error: " + _e.Message);
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
        M.ApplicationError("Nancy web server stop error: " + _e.Message);
      }
    }
  }//end of class
  
}
