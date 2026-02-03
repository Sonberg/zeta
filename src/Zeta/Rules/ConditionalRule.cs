using Zeta.Core;

namespace Zeta.Rules;

public readonly struct ConditionalRule<T> : IValidationRule<T>
{
    private readonly Func<T, bool> _condition;
    private readonly IValidationRule<T> _rule;

    public ConditionalRule(Func<T, bool> condition, IValidationRule<T> rule)
    {
        _condition = condition;
        _rule = rule;
    }

    public ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext context)
    {
        return _condition(value) ? _rule.ValidateAsync(value, context) : default;
    }
}

public readonly struct ConditionalRule<T, TContext> : IValidationRule<T, TContext>
{
    private readonly Func<T, TContext, bool> _condition;
    private readonly IValidationRule<T, TContext> _rule;

    public ConditionalRule(Func<T, TContext, bool> condition, IValidationRule<T, TContext> rule)
    {
        _condition = condition;
        _rule = rule;
    }

    public ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        return _condition(value, context.Data) ? _rule.ValidateAsync(value, context) : default;
    }
}
