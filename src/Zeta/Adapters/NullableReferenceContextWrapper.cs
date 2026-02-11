namespace Zeta.Adapters;

/// <summary>
/// Wraps a context-aware ISchema&lt;T, TContext&gt; to be used as ISchema&lt;T?, TContext&gt; for reference types.
/// </summary>
internal sealed class NullableReferenceContextWrapper<T, TContext> : ISchema<T?, TContext>
    where T : class
{
    private readonly ISchema<T, TContext> _inner;

    public NullableReferenceContextWrapper(ISchema<T, TContext> inner)
    {
        _inner = inner;
    }

    public bool AllowNull => _inner.AllowNull;

    public async ValueTask<Result> ValidateAsync(T? value, ValidationContext<TContext> context)
    {
        var result = await _inner.ValidateAsync(value, context);
        return result.IsSuccess
            ? Result.Success()
            : Result.Failure(result.Errors);
    }
}