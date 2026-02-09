using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Numeric;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating decimal values.
/// </summary>
public sealed class DecimalContextlessSchema : ContextlessSchema<decimal, DecimalContextlessSchema>
{
    internal DecimalContextlessSchema()
    {
    }

    public DecimalContextlessSchema Min(decimal min, string? message = null)
    {
        Use(new MinDecimalRule(min, message));
        return this;
    }

    public DecimalContextlessSchema Max(decimal max, string? message = null)
    {
        Use(new MaxDecimalRule(max, message));
        return this;
    }

    public DecimalContextlessSchema Positive(string? message = null)
    {
        Use(new PositiveDecimalRule(message));
        return this;
    }

    public DecimalContextlessSchema Negative(string? message = null)
    {
        Use(new NegativeDecimalRule(message));
        return this;
    }

    public DecimalContextlessSchema Precision(int maxDecimalPlaces, string? message = null)
    {
        Use(new PrecisionRule(maxDecimalPlaces, message));
        return this;
    }

    public DecimalContextlessSchema MultipleOf(decimal step, string? message = null)
    {
        Use(new MultipleOfRule(step, message));
        return this;
    }

    /// <summary>
    /// Creates a context-aware decimal schema with all rules from this schema.
    /// </summary>
    public DecimalContextSchema<TContext> WithContext<TContext>()
    {
        var schema = new DecimalContextSchema<TContext>(Rules.ToContext<TContext>());
        if (AllowNull) schema.Nullable();
        return schema;
    }
}