using Monik.Client.ClientControl;
using Monik.Client.Sender;
using Monik.Client.Settings;
using MonikService.Core.Cache;
using MonikService.Core.Messages;
using MonikService.Core.Repository;
using MonikService.Core.Sender;
using MonikService.Core.Settings;
using MonikWinService.Settings;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

// ReSharper disable once CheckNamespace
namespace Monik.Service
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        public static TinyIoCContainer MainContainer { get; private set; } = null;

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            //container.Resolve<IServiceSettings>().OnStart();

            container.Resolve<ISourceInstanceCache>().OnStart();
            container.Resolve<ICacheLog>().OnStart();
            //container.Resolve<ICacheKeepAlive>().OnStart();
            container.Resolve<IMessageProcessor>().OnStart();
            container.Resolve<IMessagePump>().OnStart();
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            container.Register<IServiceSettings, WinServiceSettings>().AsSingleton();

            container.Register<IRepository, Repository>();

            container.Register<ICacheLog, CacheLog>().AsSingleton();
            container.Register<ICacheKeepAlive, CacheKeepAlive>().AsSingleton();
            container.Register<ISourceInstanceCache, SourceInstanceCache>().AsSingleton();
            container.Register<ICacheMetrics, CacheMetrics>().AsSingleton();

            container.Register<IMessageProcessor, MessageProcessor>().AsSingleton();
            container.Register<IMessagePump, MessagePump>().AsSingleton();

            container.Register<IClientSender, AzureServiceSender>().AsSingleton();
            container.Register<IClientSettings, ServiceClientSettings>().AsSingleton();
            container.Register<IClientControl, MonikInstance>().AsSingleton();

            MainContainer = container;
        }
    } //end of class
}