using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Penguin.Cms.Web.Mvc.ModelBinders
{
    public class FlagsEnumModelBinder : IModelBinder
    {
        private static Task CompletedTask => Task.CompletedTask;

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext is null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            // Only accept enum values
            if (!bindingContext.ModelMetadata.IsFlagsEnum)
            {
                return CompletedTask;
            }

            ValueProviderResult provideValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            // Do nothing if there is no actual values
            if (provideValue == ValueProviderResult.None)
            {
                return CompletedTask;
            }

            // Get the real enum type
            Type enumType = bindingContext.ModelType;
            enumType = Nullable.GetUnderlyingType(enumType) ?? enumType;

            // Each value self may contains a series of actual values, split it with comma
            System.Collections.Generic.IEnumerable<string> strs = provideValue.Values.SelectMany(s => s.Split(','));

            // Convert all items into enum items.
            System.Collections.Generic.IEnumerable<object> actualValues = strs.Select(valueString => Enum.Parse(enumType, valueString));

            // Merge to final result
            int result = actualValues.Aggregate(0, (current, value) => current | (int)value);

            // Convert to Enum object
            object realResult = Enum.ToObject(enumType, result);

            // Result
            bindingContext.Result = ModelBindingResult.Success(realResult);

            return CompletedTask;
        }
    }
}