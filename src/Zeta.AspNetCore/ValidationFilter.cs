using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Zeta.AspNetCore;

/// <summary>
/// A minimal API endpoint filter that validates the request using a Zeta schema.
/// </summary>
/// <typeparam name="T">The type of the parameter to validate.</typeparam>
public class ValidationFilter<T> : IEndpointFilter
{
    private readonly ISchema<T> _schema;

    public ValidationFilter(ISchema<T> schema)
    {
        _schema = schema;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        
        if (argument is null)
        {
            // If body is optional or compatible type not found, we might skip or fail.
            // For now, let's assume if the filter is applied, the type T is expected.
            // But T could be a service or something else? 
            // Usually WithValidation<T> implies we want to validate the T body/parameter.
             return await next(context);
        }

        var validationContext = new ValidationContext(
            path: "", 
            services: context.HttpContext.RequestServices, 
            cancellationToken: context.HttpContext.RequestAborted);

        var result = await _schema.ValidateAsync(argument, validationContext);

        if (result.IsFailure)
        {
            var errors = result.Errors
                .GroupBy(e => e.Path)
                .ToDictionary(
                    g => g.Key, 
                    g => g.Select(e => e.Message).ToArray()
                );

            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}
