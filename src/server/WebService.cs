using System;
using System.Collections.Generic;
using Monik.Client;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Hosting.Self;

namespace Monik.Service
{
	public class HelloModule : NancyModule
	{
		public HelloModule(IRepository aRepo, ICacheLog aCacheLog, ICacheKeepAlive aCacheKeepAlive, IClientControl aControl)
		{
			var repo = aRepo;
			var cacheLog = aCacheLog;
			var cacheKeepAlive = aCacheKeepAlive;
			var control = aControl;

			Get("/sources", args =>
			{
				try
				{
					List<Source> result = repo.GetAllSources();
					return Response.AsJson<Source[]>(result.ToArray());
				}
				catch (Exception ex)
				{
					control.ApplicationError($"Method /sources : {ex.Message}");
					return HttpStatusCode.InternalServerError;
				}
			});

			Get("/instances", args =>
			{
				try
				{
					List<Instance> result = repo.GetAllInstances();
					return Response.AsJson<Instance[]>(result.ToArray());
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
					List<Group> result = repo.GetAllGroupsAndFill();
					return Response.AsJson<Group[]>(result.ToArray());
				}
				catch (Exception ex)
				{
					control.ApplicationError($"Method /instances : {ex.Message}");
					return HttpStatusCode.InternalServerError;
				}
			});

			Post("/logs3", args =>
			{
				int? top = Request.Query["top"].HasValue ? Request.Query["top"] : null;
				string order = Request.Query["order"].HasValue ? Request.Query["order"] : string.Empty;
				long? lastid = Request.Query["lastid"].HasValue ? Request.Query["lastid"] : null;
				var filters = this.Bind<LogsFilter[]>();

				try
				{
					List<Log_> result = cacheLog.GetLogs(top, order == "desc" ? Order.Desc : Order.Asc, lastid, filters);
					return Response.AsJson<Log_[]>(result.ToArray());
				}
				catch (Exception ex)
				{
					control.ApplicationError($"Method /logs3 : {ex.Message}");
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
			    List<Log_> _result = _cacheLog.GetLogs4(_group, _lastid, _severityCutoff, _level, _top);
			    return Response.AsJson<Log_[]>(_result.ToArray());
			  }
			  catch (Exception _e)
			  {
			    _control.ApplicationError($"Method /logs3 : {_e.Message}");
			    return HttpStatusCode.InternalServerError;
			  }
			});*/

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
					control.ApplicationError($"Method /logs5 : {ex.Message}");
					return HttpStatusCode.InternalServerError;
				}
			});

			Post("/keepalive", args =>
			{
				var filters = this.Bind<LogsFilter[]>();

				try
				{
					List<KeepAlive_> result = cacheKeepAlive.GetKeepAlive(filters);
					return Response.AsJson<KeepAlive_[]>(result.ToArray());
				}
				catch (Exception ex)
				{
					control.ApplicationError($"Method /keepalive : {ex.Message}");
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
					control.ApplicationError($"Method /keepalive : {ex.Message}");
					return HttpStatusCode.InternalServerError;
				}
			});

			// TODO: /status
			// return: 
		}
	}

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
