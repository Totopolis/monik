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

    private QueueClient FQueueClient;
    private SourceInstanceCache FSourceInstanceCache;

    public override bool OnStart()
    {
      ServicePointManager.DefaultConnectionLimit = 12;
      bool result = base.OnStart();

      var _dbcs = CloudConfigurationManager.GetSetting("DBConnectionString");
      var _incs = CloudConfigurationManager.GetSetting("IncomingConnectionString");
      var _sbcs = CloudConfigurationManager.GetSetting("OutcomingConnectionString");
      var _incoming = CloudConfigurationManager.GetSetting("IncomingQueue");

      var _azureSender = new AzureSender(_sbcs, _incoming);
      M.Initialize(_azureSender, "Monik", "Instance1");

      // TODO: retry logic and exit if exceptions...

      FSourceInstanceCache = new SourceInstanceCache(_dbcs);
      FSourceInstanceCache.Initialize();

      M.ApplicationInfo("MonikWorker has been started");

      FQueueClient = QueueClient.CreateFromConnectionString(_incs, _incoming);

      FQueueClient.OnMessage(message =>
      {
        try
        {
          byte[] _buf = message.GetBody<byte[]>();

          Log _msg = Log.Parser.ParseFrom(_buf);

          var _sourceInstance = FSourceInstanceCache.CheckSourceAndInstance(Helper.Utf8ToUtf16(_msg.Source), Helper.Utf8ToUtf16(_msg.Instance));
          FSourceInstanceCache.WriteLog(_msg, _sourceInstance);
        }
        catch(Exception _e)
        {
          M.ApplicationError("Message processing error: " + _e.Message);
        }
      });

      return result;
    }

    public override void OnStop()
    {
      M.ApplicationInfo("MonikWorker is stopping");

      FQueueClient.Close();
      this.cancellationTokenSource.Cancel();
      this.runCompleteEvent.WaitOne();

      // TODO: lost stopped messages

      M.ApplicationInfo("MonikWorker has stopped");

      M.Stop();

      base.OnStop();
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
      // TODO: Replace the following with your own logic.
      while (!cancellationToken.IsCancellationRequested)
      {
        M.ApplicationInfo("Working");
        await Task.Delay(10000);
      }
    }
  }
}
