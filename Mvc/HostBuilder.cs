using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Penguin.Cms.Web.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Penguin.Cms.Web.Mvc
{
    public static class HostBuilder
    {
        public static string ApplicationConfig
        {
            get
            {
                string EntryName = Assembly.GetEntryAssembly().GetName().Name;
                return $"{EntryName}.json";
            }
        }

        public static void CmsJson(IConfigurationBuilder builder, string EnvironmentName, string? BasePath = null)
        {
            BasePath ??= Directory.GetCurrentDirectory();

            string environmentSettings = $"appsettings.{EnvironmentName}.json";
            string clientSettings = Path.Combine("Client", "appsettings.json");
            string clientEnvironmentSettings = Path.Combine("Client", $"appsettings.{EnvironmentName}.json");

            string EntryPath = Path.Combine(BasePath, ApplicationConfig);

            if (!File.Exists(EntryPath))
            {
                File.WriteAllText(EntryPath, "{ }");
            }

            builder.SetBasePath(BasePath)
                .AddJsonFile(ApplicationConfig, optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(environmentSettings, optional: true, reloadOnChange: true)
                .AddJsonFile(clientSettings, optional: true, reloadOnChange: true)
                .AddJsonFile(clientEnvironmentSettings, optional: true, reloadOnChange: true);

            Penguin.Debugging.StaticLogger.Log("Configuration files loaded", Penguin.Debugging.StaticLogger.LoggingLevel.Method);
        }

        //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2
        public static IHostBuilder Configure(params string[] args)
        {
            return new Microsoft.Extensions.Hosting.HostBuilder()
.UseContentRoot(Directory.GetCurrentDirectory())
.UseServiceProviderFactory(new ServiceProviderFactory())
.ConfigureAppConfiguration((ctx, config) => ConfigConfiguration(ctx, config, args));
        }

        //https://stackoverflow.com/questions/46364293/automatically-set-appsettings-json-for-dev-and-release-environments-in-asp-net-c
        private static void ConfigConfiguration(HostBuilderContext ctx, IConfigurationBuilder config, params string[] args)
        {
            CmsJson(config, ctx.HostingEnvironment.EnvironmentName);

            if (args?.Any() ?? false)
            {
                Console.WriteLine($"Args found: {string.Join(" ", args)}");

                config.AddCommandLine(args);
            }
            else
            {
                Console.WriteLine("No args found");
            }
        }
    }
}