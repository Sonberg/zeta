using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Numeric;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating decimal values.
/// </summary>
public class DecimalContextSchema<TContext> : ContextSchema<decimal, TContext, DecimalContextSchema<TContext>>
{
    internal DecimalContextSchema(ContextRuleEngine<decimal, TContext> rules) : base(rules)
    {
    }

    public DecimalContextSchema<TContext> Min(decimal min, string? message = null)
    {
        Use(new MinDecimalRule<TContext>(min, message));
        return this;
    }

    public DecimalContextSchema<TContext> Max(decimal max, string? message = null)
    {
        Use(new MaxDecimalRule<TContext>(max, message));
        return this;
    }

    public DecimalContextSchema<TContext> Positive(string? message = null)
    {
        Use(new PositiveDecimalRule<TContext>(message));
        return this;
    }

    public DecimalContextSchema<TContext> Negative(string? message = null)
    {
        Use(new NegativeDecimalRule<TContext>(message));
        return this;
    }

    public DecimalContextSchema<TContext> Precision(int maxDecimalPlaces, string? message = null)
    {
        Use(new PrecisionRule<TContext>(maxDecimalPlaces, message));
        return this;
    }

    public DecimalContextSchema<TContext> MultipleOf(decimal step, string? message = null)
    {
        Use(new MultipleOfRule<TContext>(step, message));
        return this;
    }
}