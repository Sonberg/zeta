using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Zeta.AspNetCore;

/// <summary>
/// A minimal API endpoint filter that validates the request using a Zeta schema and optional context factory.
/// </summary>
public class ValidationFilter<T, TContext> : IEndpointFilter
{
    private readonly ISchema<T, TContext> _schema;
    private readonly IValidationContextFactory<T, TContext>? _factory;

    public ValidationFilter(ISchema<T, TContext> schema, IValidationContextFactory<T, TContext>? factory = null)
    {
        _schema = schema;
        _factory = factory;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        
        if (argument is null)
        {
             return await next(context);
        }

        var executionContext = new ValidationExecutionContext(
            path: "", 
            services: context.HttpContext.RequestServices, 
            cancellationToken: context.HttpContext.RequestAborted);

        TContext contextData;
        if (_factory != null)
        {
            contextData = await _factory.CreateAsync(argument, executionContext.Services, executionContext.CancellationToken);
        }
        else if (typeof(TContext) == typeof(object))
        {
             contextData = (TContext)(object)null!;
        }
        else
        {
            // If TContext is not object/void but no factory provided, try resolving from DI? 
            // Or assume default? 
            // For now, let's try resolving Factory from DI if available.
             var factory = context.HttpContext.RequestServices.GetService<IValidationContextFactory<T, TContext>>();
             if (factory != null)
             {
                 contextData = await factory.CreateAsync(argument, executionContext.Services, executionContext.CancellationToken);
             }
             else
             {
                 contextData = default!;
             }
        }

        var validationContext = new ValidationContext<TContext>(contextData, executionContext);
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
