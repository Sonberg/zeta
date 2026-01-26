using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Zeta.AspNetCore;

/// <summary>
/// A minimal API endpoint filter that validates the request using a contextless Zeta schema.
/// </summary>
public class ContextlessValidationFilter<T> : IEndpointFilter
{
    private readonly ISchema<T> _schema;

    public ContextlessValidationFilter(ISchema<T> schema)
    {
        _schema = schema;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();

        if (argument is null)
        {
            return await next(context);
        }

        var validator = context.HttpContext.RequestServices.GetRequiredService<IZetaValidator>();
        var result = await validator.ValidateAsync(argument, _schema, context.HttpContext.RequestAborted);

        if (!result.IsFailure) return await next(context);

        var errors = result.Errors
            .GroupBy(e => e.Path)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Message).ToArray()
            );

        return Results.ValidationProblem(errors);
    }
}

/// <summary>
/// A minimal API endpoint filter that validates the request using a context-aware Zeta schema.
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

        var validator = context.HttpContext.RequestServices.GetRequiredService<IZetaValidator>();
        var result = _factory is not null
            ? await validator.ValidateAsync(argument, _schema, _factory, context.HttpContext.RequestAborted)
            : await validator.ValidateAsync(argument, _schema, context.HttpContext.RequestAborted);

        if (!result.IsFailure) return await next(context);

        var errors = result.Errors
            .GroupBy(e => e.Path)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Message).ToArray()
            );

        return Results.ValidationProblem(errors);
    }
}
