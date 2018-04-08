using System.Configuration;
using Autofac;
using Monik.Common;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;

namespace Monik.Service
{
    public partial class Bootstrapper : AutofacNancyBootstrapper
    {
        public static ILifetimeScope Global;

        public ILifetimeScope Container => ApplicationContainer;

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            Global = Container;
            // No registrations should be performed in here, however you may
            // resolve things that are needed during application startup.

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

        // TODO: implement stop logic !!! in nanccy hostHolder OnStop() ???

        protected override void ConfigureApplicationContainer(ILifetimeScope existingContainer)
        {
            existingContainer.RegisterSingleton<IMonikServiceSettings, MonikServiceSettings>();

            existingContainer.RegisterImplementation<IRepository, Repository>();

            existingContainer.RegisterSingleton<IMonik, MonikEmbedded>();

            existingContainer.RegisterSingleton<ISourceInstanceCache, SourceInstanceCache>();
            existingContainer.RegisterSingleton<ICacheLog, CacheLog>();
            existingContainer.RegisterSingleton<ICacheKeepAlive, CacheKeepAlive>();

            existingContainer.RegisterSingleton<IMessageProcessor, MessageProcessor>();
            existingContainer.RegisterSingleton<IMessagePump, MessagePump>();

            existingContainer.Update(builder => builder.Register(c => existingContainer));
        }

        protected override void ConfigureRequestContainer(ILifetimeScope container, NancyContext context)
        {
            // Perform registrations that should have a request lifetime
        }

        protected override void RequestStartup(ILifetimeScope container, IPipelines pipelines, NancyContext context)
        {
            // No registrations should be performed in here, however you may
            // resolve things that are needed during request startup.
        }
    }

    public static class LifeTimeExtension
    {
        public static void RegisterSingleton<TInterface, TImplementation>(this ILifetimeScope container)
        {
            container.Update(builder => builder
                .RegisterType<TImplementation>()
                .As<TInterface>()
                .SingleInstance());
        }

        public static void RegisterMultiplySingleton<TI1, TI2, TImplementation>(this ILifetimeScope container)
        {
            container.Update(builder => builder
                .RegisterType<TImplementation>()
                .As<TI1>()
                .As<TI2>()
                .SingleInstance());
        }

        public static void RegisterImplementation<TInterface, TImplementation>(this ILifetimeScope container)
        {
            container.Update(builder => builder
                .RegisterType<TImplementation>()
                .As<TInterface>());
        }

        public static void RegisterInstance<TInterface, TImplementation>(this ILifetimeScope container, TImplementation aInstance)
        {
            container.Update(builder => builder
                .Register(x => aInstance)
                .As<TInterface>());
        }

        public static void RegisterSingleInstance<TInterface, TImplementation>(this ILifetimeScope container, TImplementation aInstance)
        {
            container.Update(builder => builder
                .Register(x => aInstance)
                .As<TInterface>()
                .SingleInstance());
        }

        public static void RegisterNamedSingleton<TInterface, TImplementation>(this ILifetimeScope container, string name)
        {
            container.Update(builder => builder
                .RegisterType<TImplementation>()
                .Named<TInterface>(name)
                .SingleInstance());
        }

        public static void RegisterNamedImplementation<TInterface, TImplementation>(this ILifetimeScope container, string name)
        {
            container.Update(builder => builder
                .RegisterType<TImplementation>()
                .Named<TInterface>(name));
        }
    }//end of class
}
