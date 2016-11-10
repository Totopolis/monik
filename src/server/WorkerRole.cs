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

    public override void Run()
    {
      FControl.ApplicationVerbose("MonikWorker is running");

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
    private IClientControl FControl;

    public override bool OnStart()
    {
      ServicePointManager.DefaultConnectionLimit = 12;
      bool result = base.OnStart();

      // TODO: retry logic and exit if exceptions in Bootstrapper...

      string _prefix = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["HTTP_EP"].IPEndpoint.ToString();
      FService = new WebService(_prefix);
      FService.OnStart();

      FControl = Bootstrapper.MainContainer.Resolve<IClientControl>();

      FControl.ApplicationWarning("MonikWorker has been started");

      return result;
    }

    public override void OnStop()
    {
      FControl.ApplicationWarning("MonikWorker is stopping");

      // TODO: catch exceptions inside

      FService.OnStop();

      var container = Bootstrapper.MainContainer;
      container.Resolve<IMessagePump>().OnStop();
      container.Resolve<IMessageProcessor>().OnStop();

      this.cancellationTokenSource.Cancel();
      this.runCompleteEvent.WaitOne();

      FControl.ApplicationWarning("MonikWorker has stopped");

      FControl.OnStop();

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
