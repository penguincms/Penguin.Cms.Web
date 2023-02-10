using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Penguin.Cms.Web.Extensions;
using Penguin.Cms.Web.Mvc;
using Penguin.DependencyInjection.ServiceProviders;
using Penguin.DependencyInjection.ServiceScopes;
using Penguin.Web.DependencyInjection;
using System;
using DependencyEngine = Penguin.DependencyInjection.Engine;

namespace Penguin.Cms.Web.DependencyInjection
{
    /// <summary>
    /// A service provider factory that creates a new scoped Penguin DependencyInjector instance and
    /// populates it with the required MVC registrations from the provided service collection
    /// </summary>
    public class ServiceProviderFactory : IServiceProviderFactory<ContainerBuilder>
    {
        /// <summary>
        /// Creates a dummy container builder and registers the required services
        /// </summary>
        /// <param name="services">The MVC service collection</param>
        /// <returns>A dummy container builder</returns>
        public ContainerBuilder CreateBuilder(IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            DependencyEngine.Register<IServiceScopeFactory, PerRequestScopeFactory>();
            DependencyEngine.Register<IServiceProvider, DependencyEngine>(typeof(ScopedServiceProvider));
            DependencyEngine.Register<HttpContext, DefaultHttpContext>(typeof(ScopedServiceProvider));
            DependencyEngine.Register((IServiceProvider serviceProvider) => serviceProvider.GetService<HttpContext>().TryGetSession(), typeof(ScopedServiceProvider));
            DependencyEngine.Register<IControllerFactory, ControllerFactory>(typeof(SingletonServiceProvider));

            foreach (ServiceDescriptor descriptor in services)
            {
                Type? lifeTime = null;

                switch (descriptor.Lifetime)
                {
                    case ServiceLifetime.Scoped:
                        lifeTime = typeof(ScopedServiceProvider);
                        break;

                    case ServiceLifetime.Transient:
                        lifeTime = typeof(TransientServiceProvider);
                        break;

                    case ServiceLifetime.Singleton:
                        lifeTime = typeof(SingletonServiceProvider);
                        break;
                }

                if (descriptor.ImplementationFactory != null)
                {
                    DependencyEngine.Register(descriptor.ServiceType, descriptor.ImplementationFactory, lifeTime);
                }
                else if (descriptor.ImplementationInstance != null)
                {
                    DependencyEngine.RegisterInstance(descriptor.ServiceType, descriptor.ImplementationInstance);
                }
                else
                {
                    DependencyEngine.Register(descriptor.ServiceType, descriptor.ImplementationType, lifeTime);
                }
            }

            return new ContainerBuilder();
        }

        /// <summary>
        /// Creates a new instance of the Scoped Service Provider
        /// </summary>
        /// <param name="containerBuilder">The dummy container builder</param>
        /// <returns>A new scoped service provider</returns>
        public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
        {
            using ScopedServiceScope scoped = new();
            return scoped.ServiceProvider;
        }
    }
}