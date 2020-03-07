using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Penguin.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Penguin.Cms.Web.Providers
{
    public class RCLStaticFiles : IPostConfigureOptions<StaticFileOptions>
    {
        private readonly IHostingEnvironment _environment;
        public static IFileProvider FileProvider { get; set; }

        public RCLStaticFiles(IHostingEnvironment environment)
        {
            this._environment = environment;
        }

        public void PostConfigure(string name, StaticFileOptions options)
        {
            // Basic initialization in case the options weren't initialized by any other component
            options.ContentTypeProvider ??= new FileExtensionContentTypeProvider();

            if (options.FileProvider == null && this._environment.WebRootFileProvider == null)
            {
                throw new InvalidOperationException("Missing FileProvider.");
            }

            options.FileProvider ??= this._environment.WebRootFileProvider;

            List<IFileProvider> providers = new List<IFileProvider>()
            {
                options.FileProvider
            };

            List<Assembly> allAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            foreach (Assembly a in allAssemblies)
            {
                Type t = typeof(Microsoft.Extensions.FileProviders.Embedded.EmbeddedResourceFileInfo);
                StaticLogger.Log(t.FullName);

                try
                {
                    if (!a.IsDynamic)
                    {
                        if (a.GetManifestResourceStream("Microsoft.Extensions.FileProviders.Embedded.Manifest.xml") != null)
                        {
                            providers.Add(new ManifestEmbeddedFileProvider(a, "wwwroot"));
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            FileProvider = new CompositeFileProvider(providers);
            options.FileProvider = FileProvider;
        }
    }
}