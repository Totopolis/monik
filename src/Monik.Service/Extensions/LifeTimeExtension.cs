using Autofac;
using Nancy.Bootstrappers.Autofac;

namespace Monik.Service
{
    public static class LifeTimeExtension
    {
        public static void RegisterSingleton<TInterface, TImplementation>(this ILifetimeScope container)
        {
            container.Update(builder => RegistrationExtensions.RegisterType<TImplementation>(builder)
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
    }
}