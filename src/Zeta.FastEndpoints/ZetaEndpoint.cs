using FastEndpoints;

namespace Zeta.FastEndpoints;

/// <summary>
/// Base class for FastEndpoints endpoints with Zeta schema validation.
/// Call <see cref="Validate"/> inside <see cref="BaseEndpoint.Configure"/> to register a schema.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
public abstract class ZetaEndpoint<TRequest> : Endpoint<TRequest>
    where TRequest : notnull
{
    /// <summary>
    /// Registers a <see cref="ZetaPreProcessor{TRequest}"/> for the given schema.
    /// Handles both contextless and context-aware schemas.
    /// </summary>
    protected void Validate(ISchema<TRequest> schema)
        => PreProcessors(new ZetaPreProcessor<TRequest>(schema));
}

/// <summary>
/// Base class for FastEndpoints endpoints with Zeta schema validation.
/// Call <see cref="Validate"/> inside <see cref="BaseEndpoint.Configure"/> to register a schema.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract class ZetaEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{
    /// <summary>
    /// Registers a <see cref="ZetaPreProcessor{TRequest}"/> for the given schema.
    /// Handles both contextless and context-aware schemas.
    /// </summary>
    protected void Validate(ISchema<TRequest> schema)
        => PreProcessors(new ZetaPreProcessor<TRequest>(schema));
}
