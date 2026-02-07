using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Numeric;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating double values.
/// </summary>
public sealed class DoubleContextlessSchema : ContextlessSchema<double, DoubleContextlessSchema>
{
    internal DoubleContextlessSchema()
    {
    }

    public DoubleContextlessSchema Min(double min, string? message = null)
    {
        Use(new MinDoubleRule(min, message));
        return this;
    }

    public DoubleContextlessSchema Max(double max, string? message = null)
    {
        Use(new MaxDoubleRule(max, message));
        return this;
    }

    public DoubleContextlessSchema Positive(string? message = null)
    {
        Use(new PositiveDoubleRule(message));
        return this;
    }

    public DoubleContextlessSchema Negative(string? message = null)
    {
        Use(new NegativeDoubleRule(message));
        return this;
    }

    public DoubleContextlessSchema Finite(string? message = null)
    {
        Use(new FiniteRule(message));
        return this;
    }

    /// <summary>
    /// Creates a context-aware double schema with all rules from this schema.
    /// </summary>
    public DoubleContextSchema<TContext> WithContext<TContext>() => new(Rules.ToContext<TContext>());
}