namespace Zeta.Schemas;

/// <summary>
/// Adapts a contextless ISchema&lt;T&gt; to be used as ISchema&lt;T, TContext&gt;.
/// The context is ignored when validating.
/// </summary>
internal sealed class SchemaAdapter<T, TContext> : ISchema<T, TContext>
{
    private readonly ISchema<T> _inner;

    public SchemaAdapter(ISchema<T> inner)
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