using Zeta.Core;

namespace Zeta.AspNetCore;

/// <summary>
/// Injectable validator service for manual validation in controllers.
/// </summary>
public interface IZetaValidator
{
    /// <summary>
    /// Validates a value using a schema resolved from DI.
    /// </summary>
    Task<Result<T>> ValidateAsync<T>(T value, CancellationToken ct = default);

    /// <summary>
    /// Validates a value using the provided schema.
    /// </summary>
    Task<Result<T>> ValidateAsync<T>(T value, ISchema<T> schema, CancellationToken ct = default);

    /// <summary>
    /// Validates a value with context using a schema resolved from DI.
    /// </summary>
    Task<Result<T>> ValidateAsync<T, TContext>(T value, CancellationToken ct = default);

    /// <summary>
    /// Validates a value with context using the provided schema.
    /// </summary>
    Task<Result<T>> ValidateAsync<T, TContext>(T value, ISchema<T, TContext> schema, CancellationToken ct = default);

    /// <summary>
    /// Validates a value with context using the provided schema and factory.
    /// </summary>
    public Task<Result<T>> ValidateAsync<T, TContext>(
        T value,
        ISchema<T, TContext> schema,
        IValidationContextFactory<T, TContext> factory,
        CancellationToken ct = default);
}

/// <summary>
/// Default implementation of IZetaValidator.
/// </summary>
public sealed class ZetaValidator : IZetaValidator
{
    private readonly IServiceProvider _services;

    public ZetaValidator(IServiceProvider services)
    {
        _services = services;
    }

    public Task<Result<T>> ValidateAsync<T>(T value, CancellationToken ct = default)
    {
        var schema = _services.GetService(typeof(ISchema<T>)) as ISchema<T>
                     ?? throw new InvalidOperationException($"No ISchema<{typeof(T).Name}> registered in DI.");

        return ValidateAsync(value, schema, ct);
    }

    public Task<Result<T>> ValidateAsync<T>(T value, ISchema<T> schema, CancellationToken ct = default)
    {
        var context = new ValidationExecutionContext("", _services, ct);
        return schema.ValidateAsync(value, context);
    }

    public async Task<Result<T>> ValidateAsync<T, TContext>(T value, CancellationToken ct = default)
    {
        var schema = _services.GetService(typeof(ISchema<T, TContext>)) as ISchema<T, TContext>
                     ?? throw new InvalidOperationException($"No ISchema<{typeof(T).Name}, {typeof(TContext).Name}> registered in DI.");

        return await ValidateAsync(value, schema, ct);
    }

    public async Task<Result<T>> ValidateAsync<T, TContext>(T value, ISchema<T, TContext> schema, CancellationToken ct = default)
    {
        if (_services.GetService(typeof(IValidationContextFactory<T, TContext>)) is IValidationContextFactory<T, TContext> factory)
        {
            return await ValidateAsync(value, schema, factory, ct);
        }

        // No factory registered - only allow if TContext is object? (no context needed)
        if (typeof(TContext) != typeof(object))
        {
            throw new InvalidOperationException(
                $"No IValidationContextFactory<{typeof(T).Name}, {typeof(TContext).Name}> registered in DI. " +
                $"Register a factory or use a schema without context.");
        }

        var executionContext = new ValidationExecutionContext("", _services, ct);
        return await schema.ValidateAsync(value, new ValidationContext<TContext>(default!, executionContext));
    }

    public async Task<Result<T>> ValidateAsync<T, TContext>(
        T value,
        ISchema<T, TContext> schema,
        IValidationContextFactory<T, TContext> factory,
        CancellationToken ct = default)
    {
        var executionContext = new ValidationExecutionContext("", _services, ct);
        var contextData = await factory.CreateAsync(value, _services, ct);

        return await schema.ValidateAsync(value, new ValidationContext<TContext>(contextData, executionContext));
    }
}