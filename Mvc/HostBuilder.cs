using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Penguin.Cms.Web.DependencyInjection;
using System.IO;

namespace Penguin.Cms.Web.Mvc
{
    public static class HostBuilder
    {
        public static void CmsJson(IConfigurationBuilder builder, string EnvironmentName, string? BasePath = null)
        {
            BasePath ??= Directory.GetCurrentDirectory();

            string environmentSettings = $"appsettings.{EnvironmentName}.json";
            string clientSettings = Path.Combine("Client", "appsettings.json");
            string clientEnvironmentSettings = Path.Combine("Client", $"appsettings.{EnvironmentName}.json");

            builder.SetBasePath(BasePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(environmentSettings, optional: true, reloadOnChange: true)
                .AddJsonFile(clientSettings, optional: true, reloadOnChange: true)
                .AddJsonFile(clientEnvironmentSettings, optional: true, reloadOnChange: true);

            Penguin.Debugging.StaticLogger.Log("Configuration files loaded", Penguin.Debugging.StaticLogger.LoggingLevel.Method);
        }

        //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2
        public static IHostBuilder Configure()
        {
            return new Microsoft.Extensions.Hosting.HostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseServiceProviderFactory(new ServiceProviderFactory())
                .ConfigureAppConfiguration(ConfigConfiguration);
        }

        //https://stackoverflow.com/questions/46364293/automatically-set-appsettings-json-for-dev-and-release-environments-in-asp-net-c
        private static void ConfigConfiguration(HostBuilderContext ctx, IConfigurationBuilder config)
        {
            CmsJson(config, ctx.HostingEnvironment.EnvironmentName);
        }
    }
}