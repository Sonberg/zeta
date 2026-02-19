namespace Zeta.Adapters;

/// <summary>
/// Adapts a non-null contextless ISchema&lt;T&gt; to be used as ISchema&lt;T?, TContext&gt; for reference types.
/// </summary>
internal sealed class NullableReferenceContextAdapter<T, TContext> : ISchema<T?, TContext>
    where T : class
{
    private readonly ISchema<T> _inner;

    public NullableReferenceContextAdapter(ISchema<T> inner)
    {
        _inner = inner;
    }

    public bool AllowNull => _inner.AllowNull;

    IEnumerable<Func<T?, IServiceProvider, CancellationToken, ValueTask<TContext>>> ISchema<T?, TContext>.GetContextFactories()
    {
        return [];
    }

    public async ValueTask<Result<T?, TContext>> ValidateAsync(T? value, ValidationContext<TContext> context)
    {
        var result = await _inner.ValidateAsync(value, context);
        return result.IsSuccess
            ? Result<T?, TContext>.Success(value!, context.Data)
            : Result<T?, TContext>.Failure(result.Errors);
    }
}
