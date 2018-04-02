using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Monik.Common;
using Monik.Client;

namespace Monik.Service
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        public static TinyIoCContainer MainContainer { get; private set; } = null;

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            Nancy.Json.JsonSettings.MaxJsonLength = int.MaxValue;
            Nancy.Json.JsonSettings.MaxRecursions = 100;
            Nancy.Json.JsonSettings.RetainCasing = true;

            container.Resolve<IMonikServiceSettings>().OnStart();

            container.Resolve<ISourceInstanceCache>().OnStart();
            container.Resolve<ICacheLog>().OnStart();
            container.Resolve<ICacheKeepAlive>().OnStart();
            container.Resolve<IMessageProcessor>().OnStart();
            container.Resolve<IMessagePump>().OnStart();
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            container.Register<IMonikServiceSettings, ServiceSettings>().AsSingleton();

            container.Register<IRepository, Repository>();

            container.Register<ICacheLog, CacheLog>().AsSingleton();
            container.Register<ICacheKeepAlive, CacheKeepAlive>().AsSingleton();
            container.Register<ISourceInstanceCache, SourceInstanceCache>().AsSingleton();

            container.Register<IMessageProcessor, MessageProcessor>().AsSingleton();
            container.Register<IMessagePump, MessagePump>().AsSingleton();

            container.Register<IMonikSender, ServiceSender>().AsSingleton();
            container.Register<IMonikSettings, ServiceClientSettings>().AsSingleton();
            container.Register<IMonik, MonikInstance>().AsSingleton();

            MainContainer = container;
        }
    } //end of class
}
