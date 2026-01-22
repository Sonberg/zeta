using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
        // Factory will be resolved from DI
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
}
