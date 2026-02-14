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

    public IEnumerable<Func<TBase, IServiceProvider, CancellationToken, ValueTask<TContext>>> GetContextFactories()
    {
        foreach (var factory in _inner.GetContextFactories())
        {
            yield return (value, sp, ct) =>
            {
                if (value is TDerived derived)
                {
                    return factory(derived, sp, ct);
                }

                // If value is not TDerived, we can't use this factory.
                // In a polymorphic scenario where this is the only factory, this will fail.
                // This is expected if the context is only available/meaningful for TDerived.
                throw new InvalidOperationException(
                    $"Context factory for {typeof(TDerived).Name} was called with a value of type {value?.GetType().Name ?? "null"}, which is not {typeof(TDerived).Name}. " +
                    "Ensure a root context factory is provided or all branches handle their required context.");
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