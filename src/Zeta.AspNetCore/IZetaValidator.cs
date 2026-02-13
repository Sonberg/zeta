namespace Zeta.AspNetCore;

/// <summary>
/// Injectable validator service for manual validation in controllers.
/// </summary>
public interface IZetaValidator
{
    /// <summary>
    /// Validates a value using a schema resolved from DI.
    /// </summary>
    ValueTask<Result<T>> ValidateAsync<T>(T value, CancellationToken ct = default);

    /// <summary>
    /// Validates a value using the provided schema.
    /// </summary>
    ValueTask<Result<T>> ValidateAsync<T>(T value, ISchema<T> schema, CancellationToken ct = default);

    /// <summary>
    /// Validates a value with context using the provided schema.
    /// Uses the schema's built-in factory delegate to create context data.
    /// </summary>
    ValueTask<Result<T>> ValidateAsync<T, TContext>(T value, ISchema<T, TContext> schema, CancellationToken ct = default);
}

/// <summary>
/// Default implementation of IZetaValidator.
/// </summary>
public sealed class ZetaValidator : IZetaValidator
{
    private readonly IServiceProvider _services;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZetaValidator"/> class.
    /// </summary>
    /// <param name="services">The service provider for resolving dependencies.</param>
    /// <param name="timeProvider">Optional time provider for validation context. Defaults to <see cref="TimeProvider.System"/>.</param>
    public ZetaValidator(IServiceProvider services, TimeProvider? timeProvider = null)
    {
        _services = services;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public ValueTask<Result<T>> ValidateAsync<T>(T value, CancellationToken ct = default)
    {
        var schema = _services.GetService(typeof(ISchema<T>)) as ISchema<T>
                     ?? throw new InvalidOperationException($"No ISchema<{typeof(T).Name}> registered in DI.");

        return ValidateAsync(value, schema, ct);
    }

    /// <inheritdoc />
    public ValueTask<Result<T>> ValidateAsync<T>(T value, ISchema<T> schema, CancellationToken ct = default)
    {
        return schema.ValidateAsync(value, new ValidationContext(_timeProvider, ct));
    }

    /// <inheritdoc />
    public async ValueTask<Result<T>> ValidateAsync<T, TContext>(T value, ISchema<T, TContext> schema, CancellationToken ct = default)
    {
        var factory = ResolveContextFactory(schema);
        var contextData = await factory(value, _services, ct);
        var result = await schema.ValidateAsync(value, new ValidationContext<TContext>(contextData, _timeProvider, ct));
        return result.IsSuccess ? Result<T>.Success(value) : Result<T>.Failure(result.Errors);
    }

    private static Func<T, IServiceProvider, CancellationToken, Task<TContext>> ResolveContextFactory<T, TContext>(ISchema<T, TContext> schema)
    {
        var factories = schema.GetContextFactories().ToList();
        if (factories.Count == 1) return factories[0];
        if (factories.Count > 1)
        {
            throw new InvalidOperationException(
                $"Multiple context factories for {typeof(T).Name}/{typeof(TContext).Name} were found in the schema tree. " +
                "Define exactly one factory via .Using<TContext>(factory).");
        }

        throw new InvalidOperationException(
            $"No context factory for {typeof(T).Name}/{typeof(TContext).Name}. " +
            $"Provide a factory via .Using<TContext>(factory).");
    }
}
