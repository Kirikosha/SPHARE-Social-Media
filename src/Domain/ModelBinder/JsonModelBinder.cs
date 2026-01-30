using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace Domain.ModelBinder;
public class JsonModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (value == ValueProviderResult.None)
            return Task.CompletedTask;

        var rawValue = value.FirstValue;

        if (string.IsNullOrEmpty(rawValue))
            return Task.CompletedTask;

        try
        {
            var result = JsonSerializer.Deserialize(rawValue, bindingContext.ModelType);
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        catch
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                $"Invalid JSON for {bindingContext.ModelName}."
            );
        }

        return Task.CompletedTask;
    }
}
