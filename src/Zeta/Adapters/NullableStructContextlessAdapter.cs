namespace Zeta.Adapters;

/// <summary>
/// Adapts a non-null contextless ISchema&lt;T&gt; to be used as ISchema&lt;T?&gt; for value types.
/// </summary>
internal sealed class NullableStructContextlessAdapter<T> : ISchema<T?>
    where T : struct
{
    private readonly ISchema<T> _inner;

    public NullableStructContextlessAdapter(ISchema<T> inner)
    {
        _inner = inner;
    }

    public bool AllowNull => _inner.AllowNull;

    public async ValueTask<Result<T?>> ValidateAsync(T? value, ValidationContext context)
    {
        if (value is null)
        {
            return AllowNull 
                ? Result<T?>.Success(null) 
                : Result<T?>.Failure([new ValidationError(context.Path, "null_value", "Value cannot be null")]);
        }

        var result = await _inner.ValidateAsync(value.Value, context);
        return result.IsSuccess
            ? Result<T?>.Success(result.Value)
            : Result<T?>.Failure(result.Errors);
    }
}