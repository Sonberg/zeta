namespace Zeta.Adapters;

internal sealed class TypeNarrowingSchemaAdapter<TBase, TDerived, TContext> : ISchema<TBase, TContext>
    where TDerived : class, TBase
{
    private readonly ISchema<TDerived, TContext> _inner;

    public TypeNarrowingSchemaAdapter(ISchema<TDerived, TContext> inner)
    {
        _inner = inner;
    }

    bool ISchema<TBase, TContext>.AllowNull => _inner.AllowNull;

    public IEnumerable<Func<TBase, IServiceProvider, CancellationToken, Task<TContext>>> GetContextFactories()
    {
        foreach (var factory in _inner.GetContextFactories())
        {
            yield return (value, sp, ct) =>
            {
                if (value is not TDerived derived)
                {
                    throw new InvalidOperationException(
                        $"Value is of type '{typeof(TBase).Name}', not {typeof(TDerived).Name}.");
                }


                return factory(derived, sp, ct);
            };
        }
    }

    public async ValueTask<Result> ValidateAsync(TBase? value, ValidationContext<TContext> context)
    {
        if (value is not TDerived derived)
            return Result.Success();

        return await _inner.ValidateAsync(derived, context);
    }
}