using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using Microsoft.ServiceBus.Messaging;
using Monik.Service;
using Monik.Client;
using Monik.Common;

namespace MonikWorker
{
  public class WorkerRole : RoleEntryPoint
  {
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

    // TODO: ? up GRPC endpoint for incoming connections from ++, ios, andr apps

    public override void Run()
    {
      M.ApplicationInfo("MonikWorker is running");

      try
      {
        this.RunAsync(this.cancellationTokenSource.Token).Wait();
      }
      finally
      {
        this.runCompleteEvent.Set();
      }
    }

    private WebService FService;

    public override bool OnStart()
    {
      ServicePointManager.DefaultConnectionLimit = 12;
      bool result = base.OnStart();

      var _dbcs = CloudConfigurationManager.GetSetting("DBConnectionString");

      Repository.ConnectionString = _dbcs;

      Settings.DBConnectionString = _dbcs;
      Settings.CheckUpdates();

      string _instanceName = RoleEnvironment.IsEmulated ? "Development" : "Production";

      var _azureSender = new AzureSender(Settings.GetValue("OutcomingConnectionString"), Settings.GetValue("OutcomingQueue"));
      M.Initialize(_azureSender, "Monik", _instanceName);

      M.MainInstance.AutoKeepAliveInterval = 10000;
      M.MainInstance.AutoKeepAlive = true;

      // TODO: retry logic and exit if exceptions...

      var container = Nancy.TinyIoc.TinyIoCContainer.Current;
      container.Register<IRepository, Repository>();
      container.Register<IDataCache, DataCache>().AsSingleton();
      container.Register<ISourceInstanceCache, SourceInstanceCache>().AsSingleton();

      container.Resolve<ISourceInstanceCache>().OnStart();
      container.Resolve<IDataCache>().OnStart();

      container.Register<IMessageProcessor, MessageProcessor>().AsSingleton();
      container.Resolve<IMessageProcessor>().OnStart();

      container.Register<IMessagePump, MessagePump>().AsSingleton();
      container.Resolve<IMessagePump>().OnStart();

      string _prefix = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["HTTP_EP"].IPEndpoint.ToString();
      FService = new WebService(_prefix);
      FService.OnStart();

      M.ApplicationWarning("MonikWorker has been started");

      return result;
    }

    public override void OnStop()
    {
      M.ApplicationWarning("MonikWorker is stopping");

      // TODO: catch exceptions inside

      FService.OnStop();

      var container = Nancy.TinyIoc.TinyIoCContainer.Current;
      container.Resolve<IMessagePump>().OnStop();
      container.Resolve<IMessageProcessor>().OnStop();

      this.cancellationTokenSource.Cancel();
      this.runCompleteEvent.WaitOne();

      M.ApplicationWarning("MonikWorker has stopped");

      M.OnStop();

      base.OnStop();
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
      // TODO: Replace the following with your own logic.
      while (!cancellationToken.IsCancellationRequested)
      {
        //M.ApplicationInfo("Working");
        await Task.Delay(10000);
      }
    }
  }
}
