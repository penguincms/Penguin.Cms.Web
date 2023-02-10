using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace Penguin.Cms.Web.Mvc.ModelBinders
{
    public class FlagsEnumModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            return context == null
                ? throw new ArgumentNullException(nameof(context))
                : context.Metadata.IsFlagsEnum ? new FlagsEnumModelBinder() : (IModelBinder?)null;
        }
    }
}