using Zeta.Core;
using Zeta.Rules.Numeric;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Decimal validators. Work on both contextless and context-aware decimal schemas.
/// </summary>
public static class DecimalSchemaExtensions
{
    /// <summary>Requires the value to be greater than or equal to <paramref name="min"/>.</summary>
    public static TSelf Min<TSelf>(this IValueSchema<decimal, TSelf> schema, decimal min, string? message = null)
        where TSelf : IValueSchema<decimal, TSelf>
        => schema.AppendRule(new MinDecimalRule(min, message));

    /// <summary>Requires the value to be less than or equal to <paramref name="max"/>.</summary>
    public static TSelf Max<TSelf>(this IValueSchema<decimal, TSelf> schema, decimal max, string? message = null)
        where TSelf : IValueSchema<decimal, TSelf>
        => schema.AppendRule(new MaxDecimalRule(max, message));

    /// <summary>Requires the value to fall within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public static TSelf Range<TSelf>(this IValueSchema<decimal, TSelf> schema, decimal min, decimal max, string? message = null)
        where TSelf : IValueSchema<decimal, TSelf>
    {
        if (min > max)
            throw new ArgumentOutOfRangeException(nameof(min), "min must be less than or equal to max.");

        return schema.Min(min, message).Max(max, message);
    }

    /// <summary>Requires the value to be positive (greater than 0).</summary>
    public static TSelf Positive<TSelf>(this IValueSchema<decimal, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<decimal, TSelf>
        => schema.AppendRule(new PositiveDecimalRule(message));

    /// <summary>Requires the value to be negative (less than 0).</summary>
    public static TSelf Negative<TSelf>(this IValueSchema<decimal, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<decimal, TSelf>
        => schema.AppendRule(new NegativeDecimalRule(message));

    /// <summary>Requires the value to have at most <paramref name="maxDecimalPlaces"/> decimal places.</summary>
    public static TSelf Precision<TSelf>(this IValueSchema<decimal, TSelf> schema, int maxDecimalPlaces, string? message = null)
        where TSelf : IValueSchema<decimal, TSelf>
        => schema.AppendRule(new PrecisionRule(maxDecimalPlaces, message));

    /// <summary>Requires the value to be a multiple of <paramref name="step"/>.</summary>
    public static TSelf MultipleOf<TSelf>(this IValueSchema<decimal, TSelf> schema, decimal step, string? message = null)
        where TSelf : IValueSchema<decimal, TSelf>
        => schema.AppendRule(new MultipleOfRule(step, message));
}
