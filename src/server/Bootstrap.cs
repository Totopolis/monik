#if (EMULATOR)
using Monik.Service.Test;
#endif

using Autofac;
using Monik.Common;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Gzip;
using Nancy.Json;

namespace Monik.Service
{
    public partial class Bootstrapper : AutofacNancyBootstrapper
    {
        public static Bootstrapper Singleton;

        public T Resolve<T>() => ApplicationContainer.Resolve<T>();

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            if (Singleton != null)
                throw new BootstrapperException("Duplicate");

            Singleton = this;

            // No registrations should be performed in here, however you may
            // resolve things that are needed during application startup.

            // Enable Compression with Default Settings
            pipelines.EnableGzipCompression();

            JsonSettings.MaxJsonLength = int.MaxValue;
            JsonSettings.MaxRecursions = 100;
            JsonSettings.RetainCasing = true;

            var userIdentityProvider = container.Resolve<IUserIdentityProvider>();
            var configuration = new StatelessAuthenticationConfiguration(userIdentityProvider.GetUserIdentity);
            StatelessAuthentication.Enable(pipelines, configuration);

            container.Resolve<IMonikServiceSettings>().OnStart();
            container.Resolve<ISourceInstanceCache>().OnStart();
            container.Resolve<ICacheLog>().OnStart();
            container.Resolve<ICacheKeepAlive>().OnStart();
            container.Resolve<ICacheMetric>().OnStart();

            container.Resolve<IMessageProcessor>().OnStart();
            container.Resolve<IMessagePump>().OnStart();

#if (EMULATOR)
            container.Resolve<MessageEmulator>().OnStart();
#endif
        }

        // Raise at NancyHostHolder.Stop() when service shutdown
        public void OnApplicationStop()
        {
#if (EMULATOR)
            Singleton.Resolve<MessageEmulator>().OnStop();
#endif
            Singleton.Resolve<IMonik>().OnStop();
            Singleton.Resolve<IMessagePump>().OnStop();
            Singleton.Resolve<IMessageProcessor>().OnStop();

            Singleton.Resolve<ICacheMetric>().OnStop();
            Singleton.Resolve<ICacheKeepAlive>().OnStop();
            Singleton.Resolve<ICacheLog>().OnStop();
            Singleton.Resolve<ISourceInstanceCache>().OnStop();
            Singleton.Resolve<IMonikServiceSettings>().OnStop();
        }

        protected override void ConfigureApplicationContainer(ILifetimeScope existingContainer)
        {
            existingContainer.RegisterImplementation<IUserIdentityProvider, UserIdentityProvider>();

            existingContainer.RegisterSingleton<IMonikServiceSettings, MonikServiceSettings>();

#if (EMULATOR)
            // Stub have internal state !
            existingContainer.RegisterSingleton<IRepository, RepositoryStub>();
#else
            existingContainer.RegisterImplementation<IRepository, Repository>();
#endif

            existingContainer.RegisterSingleton<IMonik, MonikEmbedded>();

            existingContainer.RegisterSingleton<ISourceInstanceCache, SourceInstanceCache>();
            existingContainer.RegisterSingleton<ICacheLog, CacheLog>();
            existingContainer.RegisterSingleton<ICacheKeepAlive, CacheKeepAlive>();
            existingContainer.RegisterSingleton<ICacheMetric, CacheMetric>();

            existingContainer.RegisterSingleton<IMessageProcessor, MessageProcessor>();
            existingContainer.RegisterSingleton<IMessagePump, MessagePump>();

            existingContainer.RegisterImplementation<IMetricObject, MetricObject>();

            existingContainer.Update(builder => builder.Register(c => existingContainer));

#if (EMULATOR)
            existingContainer.Update(builder => builder.RegisterType<MessageEmulator>());
#endif
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
