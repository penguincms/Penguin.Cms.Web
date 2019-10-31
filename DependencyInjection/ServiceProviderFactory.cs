using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Penguin.Cms.Web.Extensions;
using Penguin.Cms.Web.Mvc;
using Penguin.DependencyInjection.ServiceProviders;
using Penguin.DependencyInjection.ServiceScopes;
using Penguin.Web.DependencyInjection;
using System;
using System.Diagnostics.Contracts;
using DependencyEngine = Penguin.DependencyInjection.Engine;

namespace Penguin.Cms.Web.DependencyInjection
{
    public class ServiceProviderFactory : IServiceProviderFactory<ContainerBuilder>
    {
        public ContainerBuilder CreateBuilder(IServiceCollection services)
        {
            Contract.Requires(services != null);

            //DependencyEngine engine = new DependencyEngine();

            //DependencyEngine.DetectCircularResolution = true;

            DependencyEngine.Register<IServiceScopeFactory, PerRequestScopeFactory>();
            DependencyEngine.Register<IServiceProvider, DependencyEngine>(typeof(ScopedServiceProvider));
            DependencyEngine.Register<HttpContext, DefaultHttpContext>(typeof(ScopedServiceProvider));
            DependencyEngine.Register((IServiceProvider serviceProvider) => serviceProvider.GetService<HttpContext>().TryGetSession(), typeof(ScopedServiceProvider));
            DependencyEngine.Register<IControllerFactory, ControllerFactory>(typeof(SingletonServiceProvider));

            foreach (ServiceDescriptor descriptor in services)
            {
                Type lifeTime = null;

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

        public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
        {
            using ScopedServiceScope scoped = new ScopedServiceScope();
            return scoped.ServiceProvider;
        }
    }
}