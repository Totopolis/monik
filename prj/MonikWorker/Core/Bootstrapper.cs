using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Monik.Client;

namespace Monik.Service
{
	public class Bootstrapper : DefaultNancyBootstrapper
	{
		public static TinyIoCContainer MainContainer { get; private set; } = null;

		protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
		{
			container.Resolve<IServiceSettings>().OnStart();

			container.Resolve<ISourceInstanceCache>().OnStart();
			container.Resolve<ICacheLog>().OnStart();
			container.Resolve<ICacheKeepAlive>().OnStart();
			container.Resolve<IMessageProcessor>().OnStart();
			container.Resolve<IMessagePump>().OnStart();
		}

		protected override void ConfigureApplicationContainer(TinyIoCContainer container)
		{
			container.Register<IServiceSettings, CloudServiceSettings>().AsSingleton();

			container.Register<IRepository, Repository>();

			container.Register<ICacheLog, CacheLog>().AsSingleton();
			container.Register<ICacheKeepAlive, CacheKeepAlive>().AsSingleton();
			container.Register<ISourceInstanceCache, SourceInstanceCache>().AsSingleton();

			container.Register<IMessageProcessor, MessageProcessor>().AsSingleton();
			container.Register<IMessagePump, MessagePump>().AsSingleton();

			container.Register<IClientSender, ServiceSender>().AsSingleton();
			container.Register<IClientSettings, ServiceClientSettings>().AsSingleton();
			container.Register<IClientControl, MonikInstance>().AsSingleton();

			MainContainer = container;
		}
	} //end of class

}
