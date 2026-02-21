namespace Zeta.Core;

/// <summary>
/// A wrapper that implements <see cref="ISchema{T}"/> (contextless) but internally wraps
/// an <see cref="ISchema{T, TContext}"/> with a factory delegate. During validation, it
/// resolves the context from <see cref="IServiceProvider"/> and delegates to the inner schema.
/// </summary>
internal sealed class SelfResolvingSchema<T, TContext> : ISchema<T>
{
    private readonly ISchema<T, TContext> _inner;
    private readonly Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>> _factory;

    public SelfResolvingSchema(
        ISchema<T, TContext> inner,
        Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        _inner = inner;
        _factory = factory;
    }

    public bool AllowNull => _inner.AllowNull;

    public async ValueTask<Result<T>> ValidateAsync(T? value, ValidationContext context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result<T>.Success(value!)
                : Result<T>.Failure(new ValidationError(context.PathSegments, "null_value", "Value cannot be null"));
        }

        var serviceProvider = context.ServiceProvider
            ?? throw new InvalidOperationException(
                "IServiceProvider is required for context factory resolution. " +
                "Ensure the validation context includes a service provider.");

        var contextData = await _factory(value, serviceProvider, context.CancellationToken);
        var typedContext = new ValidationContext<TContext>(
            context.PathSegments,
            contextData,
            context.TimeProvider,
            context.CancellationToken,
            context.ServiceProvider,
            context.PathFormattingOptions);

        var result = await _inner.ValidateAsync(value, typedContext);
        return result.IsSuccess
            ? Result<T>.Success(value)
            : Result<T>.Failure(result.Errors);
    }
}
