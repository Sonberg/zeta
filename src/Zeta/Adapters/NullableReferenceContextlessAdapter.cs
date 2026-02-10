namespace Zeta.Adapters;

/// <summary>
/// Adapts a non-null contextless ISchema&lt;T&gt; to be used as ISchema&lt;T?&gt; for reference types.
/// </summary>
internal sealed class NullableReferenceContextlessAdapter<T> : ISchema<T?>
    where T : class
{
    private readonly ISchema<T> _inner;

    public NullableReferenceContextlessAdapter(ISchema<T> inner)
    {
        _inner = inner;
    }

    public bool AllowNull => _inner.AllowNull;

    public async ValueTask<Result<T?>> ValidateAsync(T? value, ValidationContext context)
    {
        var result = await _inner.ValidateAsync(value, context);
        return result.IsSuccess
            ? Result<T?>.Success(result.Value)
            : Result<T?>.Failure(result.Errors);
    }
}