using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace WebConsultasMedicas.ModelBinders;

public sealed class DateOnlyModelBinder : IModelBinder
{
    private static readonly string[] SupportedFormats =
    [
        "yyyy-MM-dd", // HTML <input type="date">
        "dd/MM/yyyy",
        "d/M/yyyy",
        "MM/dd/yyyy",
        "M/d/yyyy"
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

        if (DateOnly.TryParseExact(value, SupportedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ||
            DateOnly.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed))
        {
            bindingContext.Result = ModelBindingResult.Success(parsed);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Fecha inválida.");
        return Task.CompletedTask;
    }
}

public sealed class DateOnlyModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));
        return context.Metadata.ModelType == typeof(DateOnly) ? new DateOnlyModelBinder() : null;
    }
}

