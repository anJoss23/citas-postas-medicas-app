using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace WebConsultasMedicas.ModelBinders;

public sealed class TimeOnlyModelBinder : IModelBinder
{
    private static readonly string[] SupportedFormats =
    [
        "HH:mm", // HTML <input type="time">
        "H:mm",
        "HH:mm:ss",
        "H:mm:ss"
    ];

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext is null) throw new ArgumentNullException(nameof(bindingContext));

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrWhiteSpace(value))
        {
            return Task.CompletedTask;
        }

        if (TimeOnly.TryParseExact(value, SupportedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ||
            TimeOnly.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed))
        {
            bindingContext.Result = ModelBindingResult.Success(parsed);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Hora inválida.");
        return Task.CompletedTask;
    }
}

public sealed class TimeOnlyModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));
        return context.Metadata.ModelType == typeof(TimeOnly) ? new TimeOnlyModelBinder() : null;
    }
}

