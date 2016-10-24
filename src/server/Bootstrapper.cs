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
      container.Register<IRepository, Repository>();
      container.Register<ICacheLog, CacheLog>().AsSingleton();
      container.Register<ICacheKeepAlive, CacheKeepAlive>().AsSingleton();
      container.Register<ISourceInstanceCache, SourceInstanceCache>().AsSingleton();
      container.Register<IMessageProcessor, MessageProcessor>().AsSingleton();
      container.Register<IMessagePump, MessagePump>().AsSingleton();

      MainContainer = container;
    }
  }//end of class

}
