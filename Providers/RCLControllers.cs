using Loxifi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Penguin.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Penguin.Cms.Web.Providers
{
    public static class RCLControllers
    {
        static TypeFactory TypeFactory { get; set; } = new TypeFactory(new TypeFactorySettings());

        public static void IncludeRCLControllers(this IMvcBuilder builder)
        {
            List<Assembly> ControllerAssemblies = TypeFactory.GetDerivedTypes(typeof(Controller)).Select(t => t.Assembly).Distinct().ToList();
            foreach (Assembly a in ControllerAssemblies)
            {
                _ = builder.AddApplicationPart(a).AddControllersAsServices().AddViewComponentsAsServices();
            }
        }
    }
}