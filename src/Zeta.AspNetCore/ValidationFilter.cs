using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Zeta.AspNetCore;

/// <summary>
/// A minimal API endpoint filter that validates the request using a contextless Zeta schema.
/// </summary>
public class ContextlessValidationFilter<T> : IEndpointFilter
{
    private readonly ISchema<T> _schema;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextlessValidationFilter{T}"/> class.
    /// </summary>
    /// <param name="schema">The schema to use for validation.</param>
    public ContextlessValidationFilter(ISchema<T> schema)
    {
        _schema = schema;
    }

    /// <summary>
    /// Validates the request argument and returns a validation problem if validation fails.
    /// </summary>
    /// <param name="context">The endpoint filter invocation context.</param>
    /// <param name="next">The next filter in the pipeline.</param>
    /// <returns>A validation problem result if validation fails, otherwise the result from the next filter.</returns>
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
            .GroupBy(e => e.PathString)
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationFilter{T, TContext}"/> class.
    /// </summary>
    /// <param name="schema">The schema to use for validation.</param>
    public ValidationFilter(ISchema<T, TContext> schema)
    {
        _schema = schema;
    }

    /// <summary>
    /// Validates the request argument with context and returns a validation problem if validation fails.
    /// </summary>
    /// <param name="context">The endpoint filter invocation context.</param>
    /// <param name="next">The next filter in the pipeline.</param>
    /// <returns>A validation problem result if validation fails, otherwise the result from the next filter.</returns>
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
            .GroupBy(e => e.PathString)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Message).ToArray()
            );

        return Results.ValidationProblem(errors);
    }
}
