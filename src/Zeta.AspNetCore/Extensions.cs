using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Zeta.AspNetCore;

/// <summary>
/// Extension methods for integrating Zeta validation with ASP.NET Core Minimal APIs and MVC.
/// </summary>
public static class ZetaExtensions
{
    /// <summary>
    /// Adds validation to a minimal API endpoint using the provided contextless schema.
    /// </summary>
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder, ISchema<T> schema)
    {
        return builder.AddEndpointFilter(new ContextlessValidationFilter<T>(schema));
    }

    /// <summary>
    /// Adds validation to a minimal API endpoint using the provided context-aware schema.
    /// The schema must have a built-in factory delegate set via .Using&lt;TContext&gt;(factory).
    /// </summary>
    public static RouteHandlerBuilder WithValidation<T, TContext>(this RouteHandlerBuilder builder, ISchema<T, TContext> schema)
    {
        return builder.AddEndpointFilter(new ContextValidationFilter<T, TContext>(schema));
    }

    /// <summary>
    /// Adds validation to a minimal API endpoint using a context-aware schema that also implements ISchema&lt;T&gt;.
    /// This overload resolves ambiguity for schemas that implement both ISchema&lt;T&gt; and ISchema&lt;T, TContext&gt;.
    /// </summary>
    public static RouteHandlerBuilder WithValidation<T, TContext>(this RouteHandlerBuilder builder, IContextSchema<T, TContext> schema)
    {
        return builder.AddEndpointFilter(new ContextValidationFilter<T, TContext>(schema));
    }

    /// <summary>
    /// Converts a validation Result to an IActionResult.
    /// Returns BadRequest with ValidationProblemDetails on failure, or invokes onSuccess on success.
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, IActionResult> onSuccess)
    {
        return result.Match(
            success: onSuccess,
            failure: errors => new BadRequestObjectResult(new ValidationProblemDetails(errors.ToPathDictionary()))
        );
    }

    /// <summary>
    /// Converts a validation Result to an ActionResult&lt;TResult&gt;.
    /// Returns BadRequest with ValidationProblemDetails on failure, or invokes onSuccess on success.
    /// </summary>
    public static ActionResult<TResult> ToActionResult<T, TResult>(this Result<T> result, Func<T, ActionResult<TResult>>? onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess is not null
                ? onSuccess(result.Value)
                : new OkObjectResult(result.Value);

        return new BadRequestObjectResult(new ValidationProblemDetails(result.Errors.ToPathDictionary()));
    }

    /// <summary>
    /// Converts a validation Result to an ActionResult&lt;TResult&gt;.
    /// Returns BadRequest with ValidationProblemDetails on failure, or invokes onSuccess on success.
    /// </summary>
    public static ActionResult<TResult> ToActionResult<T, TContext, TResult>(this Result<T, TContext> result, Func<T, ActionResult<TResult>>? onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess is not null
                ? onSuccess(result.Value)
                : new OkObjectResult(result.Value);

        return new BadRequestObjectResult(new ValidationProblemDetails(result.Errors.ToPathDictionary()));
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
    /// Converts a context-aware validation Result to an IActionResult.
    /// Returns BadRequest with ValidationProblemDetails on failure, or invokes onSuccess on success.
    /// </summary>
    public static IActionResult ToActionResult<T, TContext>(this Result<T, TContext> result, Func<T, IActionResult> onSuccess)
    {
        return result.Match(
            success: onSuccess,
            failure: errors => new BadRequestObjectResult(new ValidationProblemDetails(errors.ToPathDictionary()))
        );
    }

    /// <summary>
    /// Converts a context-aware validation Result to an IActionResult.
    /// Returns BadRequest with ValidationProblemDetails on failure, or Ok with the value on success.
    /// </summary>
    public static IActionResult ToActionResult<T, TContext>(this Result<T, TContext> result)
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
            failure: errors => Results.ValidationProblem(errors.ToPathDictionary())
        );
    }

    /// <summary>
    /// Converts a validation Result to a Minimal API IResult.
    /// Returns ValidationProblem on failure, or Ok with the value on success.
    /// </summary>
    public static IResult ToResult<T>(this Result<T> result)
    {
        return result.ToResult(Results.Ok);
    }

    /// <summary>
    /// Converts a context-aware validation Result to a Minimal API IResult.
    /// Returns ValidationProblem on failure, or invokes onSuccess on success.
    /// </summary>
    public static IResult ToResult<T, TContext>(this Result<T, TContext> result, Func<T, IResult> onSuccess)
    {
        return result.Match(
            success: onSuccess,
            failure: errors => Results.ValidationProblem(errors.ToPathDictionary())
        );
    }

    /// <summary>
    /// Converts a context-aware validation Result to a Minimal API IResult.
    /// Returns ValidationProblem on failure, or Ok with the value on success.
    /// </summary>
    public static IResult ToResult<T, TContext>(this Result<T, TContext> result)
    {
        return result.ToResult(Results.Ok);
    }
}