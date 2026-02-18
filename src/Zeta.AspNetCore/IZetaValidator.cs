using Zeta.Core;

namespace Zeta.AspNetCore;

/// <summary>
/// Injectable validator service for manual validation in controllers.
/// </summary>
public interface IZetaValidator
{
    /// <summary>
    /// Validates a value using the provided schema.
    /// </summary>
    ValueTask<Result<T>> ValidateAsync<T>(T value, ISchema<T> schema, CancellationToken ct = default) => ValidateAsync(value, schema, opt => opt.WithCancellation(ct));

    /// <summary>
    /// Validates a value using the provided schema and execution context builder.
    /// </summary>
    ValueTask<Result<T>> ValidateAsync<T>(T value, ISchema<T> schema, Func<ValidationContextBuilder, ValidationContextBuilder> builder);

    /// <summary>
    /// Validates a value with context using the provided schema.
    /// Uses the schema's built-in factory delegate to create context data.
    /// </summary>
    ValueTask<Result<T>> ValidateAsync<T, TContext>(T value, ISchema<T, TContext> schema, CancellationToken ct = default) => ValidateAsync(value, schema, opt => opt.WithCancellation(ct));

    /// <summary>
    /// Validates a value with context using the provided schema and execution context builder.
    /// Uses the schema's built-in factory delegate to create context data.
    /// </summary>
    ValueTask<Result<T>> ValidateAsync<T, TContext>(T value, ISchema<T, TContext> schema, Func<ValidationContextBuilder, ValidationContextBuilder> builder);

    /// <summary>
    /// Validates a value with context using a Zeta context schema.
    /// This overload avoids ambiguity when the schema is also assignable to ISchema&lt;T&gt;.
    /// </summary>
    ValueTask<Result<T>> ValidateAsync<T, TContext>(T value, IContextSchema<T, TContext> schema, CancellationToken ct = default)
        => ValidateAsync(value, (ISchema<T, TContext>)schema, ct);

    /// <summary>
    /// Validates a value with context using a Zeta context schema and execution context builder.
    /// This overload avoids ambiguity when the schema is also assignable to ISchema&lt;T&gt;.
    /// </summary>
    ValueTask<Result<T>> ValidateAsync<T, TContext>(T value, IContextSchema<T, TContext> schema, Func<ValidationContextBuilder, ValidationContextBuilder> builder)
        => ValidateAsync(value, (ISchema<T, TContext>)schema, builder);
}

/// <summary>
/// Default implementation of IZetaValidator.
/// </summary>
public sealed class ZetaValidator : IZetaValidator
{
    private readonly IServiceProvider _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZetaValidator"/> class.
    /// </summary>
    /// <param name="services">The service provider for resolving dependencies.</param>
    public ZetaValidator(IServiceProvider services)
    {
        _services = services;
    }

    /// <inheritdoc />
    public ValueTask<Result<T>> ValidateAsync<T>(T value, ISchema<T> schema, Func<ValidationContextBuilder, ValidationContextBuilder> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return schema.ValidateAsync(value, builder(new ValidationContextBuilder().WithServiceProvider(_services)).Build());
    }


    /// <inheritdoc />
    public async ValueTask<Result<T>> ValidateAsync<T, TContext>(T value, ISchema<T, TContext> schema, Func<ValidationContextBuilder, ValidationContextBuilder> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var execution = builder(new ValidationContextBuilder().WithServiceProvider(_services)).Build();
        var contextData = await ContextFactoryResolver.ResolveAsync(
            value,
            schema.GetContextFactories(),
            _services,
            execution.CancellationToken);
        var result = await schema.ValidateAsync(
            value,
            new ValidationContext<TContext>(contextData, execution.TimeProvider, execution.CancellationToken, execution.ServiceProvider));
        return result.IsSuccess ? Result<T>.Success(value) : Result<T>.Failure(result.Errors);
    }

}
