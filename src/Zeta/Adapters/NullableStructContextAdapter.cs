namespace Zeta.Adapters;

/// <summary>
/// Adapts a non-null contextless ISchema&lt;T&gt; to be used as ISchema&lt;T?, TContext&gt; for value types.
/// </summary>
internal sealed class NullableStructContextAdapter<T, TContext> : ISchema<T?, TContext>
    where T : struct
{
    private readonly ISchema<T> _inner;

    public NullableStructContextAdapter(ISchema<T> inner)
    {
        _inner = inner;
    }

    public bool AllowNull => _inner.AllowNull;

    IEnumerable<Func<T?, IServiceProvider, CancellationToken, ValueTask<TContext>>> ISchema<T?, TContext>.GetContextFactories()
    {
        return [];
    }

    public async ValueTask<Result> ValidateAsync(T? value, ValidationContext<TContext> context)
    {
        if (value is null)
        {
            return AllowNull 
                ? Result.Success() 
                : Result.Failure([new ValidationError(context.Path, "null_value", "Value cannot be null")]);
        }

        var result = await _inner.ValidateAsync(value.Value, context);
        return result.IsSuccess
            ? Result.Success()
            : Result.Failure(result.Errors);
    }
}
