using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// Type-erased interface for contextless type assertions.
/// Used by ObjectContextlessSchema to validate polymorphic types.
/// </summary>
internal interface ITypeAssertion<T>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T value, ValidationContext context);
    
    ITypeAssertion<T, TContext> ToContext<TContext>();
}

/// <summary>
/// Type-erased interface for context-aware type assertions.
/// Used by ObjectContextSchema to validate polymorphic types.
/// </summary>
internal interface ITypeAssertion<T, TContext>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T value, ValidationContext<TContext> context);

    IEnumerable<Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>> GetContextFactories();
}

internal sealed class ContextlessTypeAssertion<T, TDerived> : ITypeAssertion<T>
    where T : class
    where TDerived : class, T
{
    private readonly ObjectContextlessSchema<TDerived> _schema;

    public ContextlessTypeAssertion(ObjectContextlessSchema<TDerived> schema)
    {
        _schema = schema;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T value, ValidationContext context)
    {
        if (value is not TDerived derived)
            return [new ValidationError(context.Path, "type_mismatch",
                $"Expected value to be of type '{typeof(TDerived).Name}' but was '{value.GetType().Name}'")];

        var result = await _schema.ValidateAsync(derived, context);
        return result.IsFailure ? result.Errors : [];
    }

    public ITypeAssertion<T, TContext> ToContext<TContext>()
        => new ContextAwareTypeAssertion<T, TDerived, TContext>(_schema.Using<TContext>());
}

internal sealed class ContextAwareTypeAssertion<T, TDerived, TContext> : ITypeAssertion<T, TContext>
    where T : class
    where TDerived : class, T
{
    private readonly ObjectContextSchema<TDerived, TContext> _schema;

    public ContextAwareTypeAssertion(ObjectContextSchema<TDerived, TContext> schema)
    {
        _schema = schema;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        if (value is not TDerived derived)
            return [new ValidationError(context.Path, "type_mismatch",
                $"Expected value to be of type '{typeof(TDerived).Name}' but was '{value.GetType().Name}'")];

        var result = await _schema.ValidateAsync(derived, context);
        return result.IsFailure ? result.Errors : [];
    }

    public IEnumerable<Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>> GetContextFactories()
    {
        foreach (var derivedFactory in ((ISchema<TDerived, TContext>)_schema).GetContextFactories())
        {
            yield return (value, services, ct) =>
            {
                if (value is not TDerived derived)
                {
                    throw new InvalidOperationException(
                        $"Factory for '{typeof(TDerived).Name}' cannot create context for '{value?.GetType().Name ?? "null"}'.");
                }

                return derivedFactory(derived, services, ct);
            };
        }
    }
}
