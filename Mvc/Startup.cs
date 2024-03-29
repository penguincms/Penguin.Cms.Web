﻿using Loxifi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Penguin.Cms.Web.Mvc.Middleware;
using Penguin.Cms.Web.Mvc.ModelBinders;
using Penguin.Cms.Web.Mvc.Routing;
using Penguin.Cms.Web.Providers;
using Penguin.Configuration.Abstractions.Interfaces;
using Penguin.Configuration.Providers;
using Penguin.Debugging;
using Penguin.Messaging.Application.Extensions;
using Penguin.Messaging.Core;
using Penguin.Persistence.Abstractions.Interfaces;
using Penguin.Reflection;
using Penguin.Testing.RuntimeValidation;
using Penguin.Web.Abstractions.Interfaces;
using Penguin.Web.DependencyInjection;
using Penguin.Web.Errors.Middleware;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using DependencyEngine = Penguin.DependencyInjection.Engine;

namespace Penguin.Cms.Web.Mvc
{
    public class Startup
    {
        public static ConcurrentBag<StartupException> Exceptions { get; } = new ConcurrentBag<StartupException>();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public static bool PersistenceConfigured { get; set; }

        private static readonly object BootLock = new();

        /// <summary>
        /// Override this method to add client services
        /// </summary>
        /// <param name="services"></param>
        public virtual void ConfigureAdditionalServices(IServiceCollection services)
        {

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                if (!Monitor.TryEnter(BootLock))
                {
                    return;
                }

                ConfigureAdditionalServices(services);

                _ = services.Configure<CookiePolicyOptions>(options =>
                {
                    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                    options.CheckConsentNeeded = context => false;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                });

                _ = services.Configure<RouteOptions>(options => options.ConstraintMap.Add("notexists", typeof(NotKnownRouteValueConstraint)));

                _ = services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromSeconds(2400);
                    options.Cookie.HttpOnly = true;
                });

                services.AddMvc(options =>
                       options.EnableEndpointRouting = false)
                       .SetCompatibilityVersion((CompatibilityVersion)3)
                       .ConfigureApplicationPartManager(RCLViews.Include)
                       .IncludeRCLControllers();

                _ = services.ConfigureOptions(typeof(RCLStaticFiles));

                _ = services.AddSingleton<IFileProvider>((ServiceProvider) => new CompositeFileProvider(RCLStaticFiles.FileProvider, new RCLViews()));

                _ = TypeFactory.Default.GetTypeByFullName("Microsoft.Extensions.DependencyInjection.MvcServiceCollectionExtensions")
                           .GetMethods().Single(m => m.Name == "AddControllersWithViews" && m.GetParameters().Length == 1)
                           .Invoke(null, new object[] { services });

                IMvcBuilder builder = services.AddMvc(options =>
                    // add custom binder to beginning of collection
                    options.ModelBinderProviders.Insert(0, new FlagsEnumModelBinderProvider()));

                Type runtimeCompilation = TypeFactory.Default.GetTypeByFullName("Microsoft.Extensions.DependencyInjection.RazorRuntimeCompilationMvcBuilderExtensions");

                if (runtimeCompilation is null)
                {
                    throw new NullReferenceException("The package Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation is not installed or is not being copied to the output directory");
                }

                _ = runtimeCompilation.GetMethods().Single(m => m.Name == "AddRazorRuntimeCompilation" && m.GetParameters().Length == 1)
                   .Invoke(null, new object[] { builder });

                _ = services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            }
            catch (Exception ex)
            {
                Exceptions.Add(new StartupException(ex));
                throw;
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            try
            {
                foreach (JsonConfigurationProvider jscp in ((IConfigurationRoot)Configuration).Providers.OfType<JsonConfigurationProvider>())
                {
                    StaticLogger.Log($"Configuration Loaded: {jscp.Source.Path}", StaticLogger.LoggingLevel.Call);
                }

                app.UseSession();

                app.UseStaticFiles();

                app.UseCookiePolicy();

                app.UseWhen(context => PersistenceConfigured, appBuilder =>
                {
                    appBuilder.UseMiddleware<ServiceScope>();

                    foreach (Type t in TypeFactory.Default.GetAllImplementations(typeof(IPenguinMiddleware)))
                    {
                        MethodInfo m = typeof(UseMiddlewareExtensions).GetMethods()
                                      .First(mi =>
                                        mi.Name == nameof(UseMiddlewareExtensions.UseMiddleware) &&
                                        !mi.ContainsGenericParameters
                                      );

                        m.Invoke(null, new object[] { appBuilder, t, Array.Empty<object>() });
                    }

                    appBuilder.UseMiddleware<ExceptionHandling>();
                });

                app.UseWhen(context => !PersistenceConfigured, appBuilder => appBuilder.UseMiddleware<ConfigurePersistenceMiddleware>());

                IProvideConfigurations provideConnectionStrings = new ConfigurationProviderList(
                        new JsonProvider(Configuration)
                );

                DependencyEngine.Register((serviceProvider) => provideConnectionStrings);

                foreach (Type t in TypeFactory.Default.GetAllImplementations(typeof(IRouteConfig)))
                {
                    if (Activator.CreateInstance(t) is IRouteConfig r)
                    {
                        app.UseMvc(r.RegisterRoutes);
                    }
                    else
                    {
                        throw new Exception("Somehow we managed to initialize a null route configuration");
                    }
                }

                app.UseMvc(routes =>
                {
                    routes.MapRoute(
                         name: "areaRoute",
                         template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}"
                    );
                });

#if DEBUG
                RuntimeValidator.ExecuteTests();
#endif

                MessageBus.SubscribeAll();

                try
                {
                    ConfigureDatabase();
                }
                catch (Exception cex)
                {
                    Exceptions.Add(new StartupException(cex));
                    PersistenceConfigured = false;
                }

                StaticLogger.Log($"Application initialization completed.", StaticLogger.LoggingLevel.Final);
            }
            catch (Exception ex)
            {
                Exceptions.Add(new StartupException(ex));
                throw;
            }
        }

        public static void ConfigureDatabase()
        {
            IPersistenceContextMigrator migrator = DependencyEngine.GetService<IPersistenceContextMigrator>();

            if (migrator is null)
            {
                throw new ArgumentNullException(nameof(migrator), "No persistence context migrator was found registered with the dependency injector, which may mean that no persistence context implementation is included in the solution");
            }

            PersistenceConfigured = migrator.IsConfigured;

            if (PersistenceConfigured)
            {
                migrator.Migrate();

                using PerRequestServiceScope ServiceScope = PerRequestScopeFactory.CreateDummy();

                new MessageBus(ServiceScope.ServiceProvider).Startup();
            }
        }
    }
}