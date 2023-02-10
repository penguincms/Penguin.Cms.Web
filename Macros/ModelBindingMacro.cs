using Penguin.Persistence.Abstractions.Attributes.Control;
using Penguin.Reflection.Abstractions;
using Penguin.Reflection.Extensions;
using Penguin.Templating.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Penguin.Cms.Web.Macros
{
    public class ModelBindingMacro : ITemplateProperty
    {
        public IList<ITemplateProperty> Children { get; }

        public string DisplayName { get; set; }

        public string MacroBody => "@(Model" + Path + "." + DisplayName + ")";

        public string Path { get; set; }

        IEnumerable<ITemplateProperty> ITemplateProperty.Children => Children;

        public ModelBindingMacro(string propertyName, Type type, string path = "", Stack<(string propertyName, Type type)>? overflowCheckHack = null)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            overflowCheckHack ??= new Stack<(string, Type)>();

            if (overflowCheckHack is null)
            {
                throw new Exception("What the fuck?");
            }

            Children = new List<ITemplateProperty>();

            DisplayName = propertyName;
            Path = path;

            if (type.GetCoreType() == CoreType.Reference)
            {
                foreach (PropertyInfo childProperty in type.GetProperties())
                {
                    string cPropertyName = childProperty.Name;
                    Type cType = childProperty.PropertyType;

                    if (overflowCheckHack.Any(o => o.propertyName == cPropertyName && o.type == cType))
                    {
                        continue;
                    }

                    if (!childProperty.GetCustomAttribute<DontAllowAttribute>()?.Context.HasFlag(DisplayContexts.TemplateBinding) ?? true)
                    {
                        overflowCheckHack.Push((cPropertyName, cType));
                        Children.Add(new ModelBindingMacro(cPropertyName, cType, path + "." + propertyName, overflowCheckHack));
                        _ = overflowCheckHack.Pop();
                    }
                }
            }
            else if (type.GetCoreType() == CoreType.Collection)
            {
                string cPropertyName = propertyName;
                Type cType = type.GetCollectionType();

                ModelBindingMacro replacement = new(cPropertyName, cType, path, overflowCheckHack);

                Children = replacement.Children;
            }
        }
    }
}