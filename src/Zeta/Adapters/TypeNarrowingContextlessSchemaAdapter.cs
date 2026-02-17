namespace Zeta.Adapters;

internal sealed class TypeNarrowingContextlessSchemaAdapter<TBase, TDerived> : ISchema<TBase>
    where TDerived : class, TBase
{
    private readonly ISchema<TDerived> _inner;

    public TypeNarrowingContextlessSchemaAdapter(ISchema<TDerived> inner)
    {
        _inner = inner;
    }

    public bool AllowNull => _inner.AllowNull;

    public async ValueTask<Result<TBase>> ValidateAsync(TBase? value, ValidationContext context)
    {
        if (value is not TDerived derived)
            return Result<TBase>.Success(value!);

        var result = await _inner.ValidateAsync(derived, context);
        return result.IsFailure 
            ? Result<TBase>.Failure(result.Errors) 
            : Result<TBase>.Success(value!);
    }
}
