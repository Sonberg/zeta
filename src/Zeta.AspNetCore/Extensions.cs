using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Zeta.AspNetCore;

public static class ZetaExtensions
{
    /// <summary>
    /// Adds validation to a minimal API endpoint using the provided schema.
    /// </summary>
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder, ISchema<T> schema)
    {
        return builder.AddEndpointFilter(new ValidationFilter<T, object?>(schema, null));
    }

    /// <summary>
    /// Adds validation to a minimal API endpoint using the provided schema and context factory.
    /// </summary>
    public static RouteHandlerBuilder WithValidation<T, TContext>(this RouteHandlerBuilder builder, ISchema<T, TContext> schema)
    {
        // Factory will be resolved from DI inside the filter if not provided
        return builder.AddEndpointFilter(new ValidationFilter<T, TContext>(schema, null));
    }

    /// <summary>
    /// Adds validation to a minimal API endpoint using the provided schema and explicit context factory.
    /// </summary>
    public static RouteHandlerBuilder WithValidation<T, TContext>(
        this RouteHandlerBuilder builder,
        ISchema<T, TContext> schema,
        IValidationContextFactory<T, TContext> factory)
    {
        return builder.AddEndpointFilter(new ValidationFilter<T, TContext>(schema, factory));
    }

    /// <summary>
    /// Converts a validation Result to an IActionResult.
    /// Returns BadRequest with ValidationProblemDetails on failure, or invokes onSuccess on success.
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, IActionResult> onSuccess)
    {
        return result.Match(
            success: onSuccess,
            failure: errors => new BadRequestObjectResult(new ValidationProblemDetails(
                errors.GroupBy(e => e.Path)
                      .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray())
            ))
        );
    }

    /// <summary>
    /// Converts a validation Result to an IActionResult.
    /// Returns BadRequest with ValidationProblemDetails on failure, or Ok with the value on success.
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        return result.ToActionResult(value => new OkObjectResult(value));
    }

    /// <summary>
    /// Converts a validation Result to a Minimal API IResult.
    /// Returns ValidationProblem on failure, or invokes onSuccess on success.
    /// </summary>
    public static IResult ToResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        return result.Match(
            success: onSuccess,
            failure: errors => Results.ValidationProblem(
                errors.GroupBy(e => e.Path)
                      .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray())
            )
        );
    }

    /// <summary>
    /// Converts a validation Result to a Minimal API IResult.
    /// Returns ValidationProblem on failure, or Ok with the value on success.
    /// </summary>
    public static IResult ToResult<T>(this Result<T> result)
    {
        return result.ToResult(value => Results.Ok(value));
    }
}
