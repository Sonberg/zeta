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

    public async ValueTask<Result> ValidateAsync(TBase? value, ValidationContext<TContext> context)
    {
        if (value is not TDerived derived)
            return Result.Success();

        return await _inner.ValidateAsync(derived, context);
    }
}
