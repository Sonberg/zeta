using Zeta.Core;
using Zeta.Rules.Numeric;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating double values.
/// </summary>
public class DoubleContextSchema<TContext> : ContextSchema<double, TContext, DoubleContextSchema<TContext>>
{
    internal DoubleContextSchema(ContextRuleEngine<double, TContext> rules) : base(rules)
    {
    }

    public DoubleContextSchema<TContext> Min(double min, string? message = null)
    {
        Use(new MinDoubleRule<TContext>(min, message));
        return this;
    }

    public DoubleContextSchema<TContext> Max(double max, string? message = null)
    {
        Use(new MaxDoubleRule<TContext>(max, message));
        return this;
    }

    public DoubleContextSchema<TContext> Positive(string? message = null)
    {
        Use(new PositiveDoubleRule<TContext>(message));
        return this;
    }

    public DoubleContextSchema<TContext> Negative(string? message = null)
    {
        Use(new NegativeDoubleRule<TContext>(message));
        return this;
    }

    public DoubleContextSchema<TContext> Finite(string? message = null)
    {
        Use(new FiniteRule<TContext>(message));
        return this;
    }
}