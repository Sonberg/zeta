namespace Zeta.Adapters;

/// <summary>
/// Wraps a context-aware ISchema&lt;T, TContext&gt; to be used as ISchema&lt;T?, TContext&gt; for value types.
/// </summary>
internal sealed class NullableStructContextWrapper<T, TContext> : ISchema<T?, TContext>
    where T : struct
{
    private readonly ISchema<T, TContext> _inner;

    public NullableStructContextWrapper(ISchema<T, TContext> inner)
    {
        _inner = inner;
    }

    public bool AllowNull => _inner.AllowNull;

    IEnumerable<Func<T?, IServiceProvider, CancellationToken, ValueTask<TContext>>> ISchema<T?, TContext>.GetContextFactories()
    {
        foreach (var factory in _inner.GetContextFactories())
        {
            yield return (value, services, ct) =>
            {
                if (!value.HasValue)
                {
                    throw new InvalidOperationException(
                        $"Factory for '{typeof(T).Name}' cannot create context for null.");
                }

                return factory(value.Value, services, ct);
            };
        }
    }

    public async ValueTask<Result<T?, TContext>> ValidateAsync(T? value, ValidationContext<TContext> context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result<T?, TContext>.Success(value!, context.Data)
                : Result<T?, TContext>.Failure([new ValidationError(context.PathSegments, "null_value", "Value cannot be null")]);
        }

        var result = await _inner.ValidateAsync(value.Value, context);
        return result.IsSuccess
            ? Result<T?, TContext>.Success(value, context.Data)
            : Result<T?, TContext>.Failure(result.Errors);
    }
}
