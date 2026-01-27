namespace Zeta.Rules;

/// <summary>
/// Adapts a contextless validation rule to work in a context-aware environment.
/// </summary>
public readonly struct ContextlessRuleAdapter<T, TContext> : IValidationRule<T, TContext>
{
    private readonly IValidationRule<T> _inner;

    public ContextlessRuleAdapter(IValidationRule<T> inner)
    {
        _inner = inner;
    }

    public ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        return _inner.ValidateAsync(value, context);
    }
}
