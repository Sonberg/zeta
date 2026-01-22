using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Zeta.AspNetCore;

/// <summary>
/// Extension methods for integrating Zeta with Minimal APIs.
/// </summary>
public static class ZetaExtensions
{
    /// <summary>
    /// Adds validation to a minimal API endpoint using the provided schema.
    /// </summary>
    /// <typeparam name="T">The type of the parameter/body to validate.</typeparam>
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder, ISchema<T> schema)
    {
        return builder.AddEndpointFilter(new ValidationFilter<T>(schema));
    }
}
