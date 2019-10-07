using System;
using Autofac;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using Monik.Common;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Gzip;

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

            var userIdentityProvider = container.Resolve<IUserIdentityProvider>();
            var configuration = new StatelessAuthenticationConfiguration(userIdentityProvider.GetUserIdentity);
            StatelessAuthentication.Enable(pipelines, configuration);

            var settings = container.Resolve<IRepository>().LoadSettings();
            container.Resolve<IMonikServiceSettings>().UpdateSettings(settings);

            container.Resolve<ICacheSourceInstance>().OnStart();
            container.Resolve<ICacheLog>().OnStart();
            container.Resolve<ICacheKeepAlive>().OnStart();
            container.Resolve<ICacheMetric>().OnStart();

            container.Resolve<IMessageProcessor>().OnStart();
            container.Resolve<IMessagePump>().OnStart();
        }

        // Raise at NancyHostHolder.Stop() when service shutdown
        public void OnApplicationStop()
        {
            Singleton.Resolve<IMonik>().OnStop();
            Singleton.Resolve<IMessagePump>().OnStop();
            Singleton.Resolve<IMessageProcessor>().OnStop();

            Singleton.Resolve<ICacheMetric>().OnStop();
            Singleton.Resolve<ICacheKeepAlive>().OnStop();
            Singleton.Resolve<ICacheLog>().OnStop();
            Singleton.Resolve<ICacheSourceInstance>().OnStop();
        }

        protected override void ConfigureApplicationContainer(ILifetimeScope existingContainer)
        {
            existingContainer.RegisterImplementation<IUserIdentityProvider, UserIdentityProvider>();

            existingContainer.RegisterSingleton<IMonikServiceSettings, MonikServiceSettings>();

            existingContainer.RegisterInstance<ContextProvider, SqlContextProvider>(SqlContextProvider.DefaultInstance);
            existingContainer.RegisterImplementation<IRepository, Repository>();

            existingContainer.RegisterSingleton<IMonik, MonikEmbedded>();

            existingContainer.RegisterSingleton<ICacheSourceInstance, CacheSourceInstance>();
            existingContainer.RegisterSingleton<ICacheLog, CacheLog>();
            existingContainer.RegisterSingleton<ICacheKeepAlive, CacheKeepAlive>();
            existingContainer.RegisterSingleton<ICacheMetric, CacheMetric>();

            existingContainer.RegisterSingleton<IMessageProcessor, MessageProcessor>();
            existingContainer.RegisterSingleton<IMessagePump, MessagePump>();

            existingContainer.RegisterImplementation<IMetricObject, MetricObject>();

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

        // Use JSON.NET serializer
        protected override Func<ITypeCatalog, NancyInternalConfiguration> InternalConfiguration =>
            NancyInternalConfiguration.WithOverrides(c => c.Serializers = new [] {typeof(JsonNetSerializer)});
    }

    //end of class
}
