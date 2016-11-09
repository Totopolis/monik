using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monik;
using Monik.Common;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Configuration;
using Nancy.TinyIoc;
using Monik.Client;

namespace Monik.Service
{
  public class Bootstrapper : DefaultNancyBootstrapper
  {
    protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
    {
    }

    public static TinyIoCContainer MainContainer { get; private set; } = null;

    protected override void ConfigureApplicationContainer(TinyIoCContainer container)
    {
      container.Register<IServiceSettings, ServiceSettings>().AsSingleton();

      container.Register<IRepository, Repository>();

      container.Register<ICacheLog, CacheLog>().AsSingleton();
      container.Register<ICacheKeepAlive, CacheKeepAlive>().AsSingleton();
      container.Register<ISourceInstanceCache, SourceInstanceCache>().AsSingleton();

      container.Register<IMessageProcessor, MessageProcessor>().AsSingleton();
      container.Register<IMessagePump, MessagePump>().AsSingleton();

      IServiceSettings _srvSett = container.Resolve<IServiceSettings>();
      _srvSett.OnStart();

      container.Register<IClientSender>(new AzureSender(_srvSett.OutcomingConnectionString, _srvSett.OutcomingQueue));
      container.Register<IClientSettings>(new ClientSettings()
      {
        SourceName = "Monik",
        InstanceName = _srvSett.CloudInstanceName,
        AutoKeepAliveEnable = true
      });

      container.Register<IClientControl, MonikInstance>().AsSingleton();

      // old school: M.Initialize(container.Resolve<IClientSender>(), "Monik", _srvSett.CloudInstanceName, aAutoKeepAliveEnable: true);

      container.Resolve<ISourceInstanceCache>().OnStart();
      container.Resolve<ICacheLog>().OnStart();
      container.Resolve<ICacheKeepAlive>().OnStart();
      container.Resolve<IMessageProcessor>().OnStart();
      container.Resolve<IMessagePump>().OnStart();

      MainContainer = container;
    }
  }//end of class

}
