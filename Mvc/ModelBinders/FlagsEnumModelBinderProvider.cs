using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace Penguin.Cms.Web.Mvc.ModelBinders
{
    public class FlagsEnumModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.IsFlagsEnum)
            {
                return new FlagsEnumModelBinder();
            }

            return null;
        }
    }
}