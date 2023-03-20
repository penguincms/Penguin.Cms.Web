using Loxifi;
using Penguin.Cms.Abstractions;
using Penguin.Cms.Abstractions.Interfaces;
using Penguin.Configuration.Abstractions.Interfaces;
using Penguin.Extensions.String;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Penguin.Cms.Web.Macros
{
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public class Configuration : IMacroProvider
    {
        protected IProvideConfigurations ConfigurationProvider { get; set; }

        public Configuration(IProvideConfigurations configurationProvider)
        {
            ConfigurationProvider = configurationProvider;
        }

        public string this[string Name] => ConfigurationProvider.GetConfiguration(Name);

        public List<Macro> GetMacros(object requester)
        {
            List<Macro> toReturn = new();

            Dictionary<string, string> allConfigs = ConfigurationProvider.AllConfigurations;

            foreach (KeyValuePair<string, string> kvp in allConfigs)
            {
                Macro macro = new(GetType().Name,
                    $"@Configuration[\"{kvp.Key}\"]"
                );

                toReturn.Add(macro);
            }

            return toReturn;
        }

        public string Resolve(string macro)
        {
            string config = macro.From(":").To("}");

            return ConfigurationProvider.GetConfiguration(config);
        }
    }
}