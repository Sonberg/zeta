using Zeta.Core;
using Zeta.Rules.Numeric;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Decimal validators. Work on both contextless and context-aware decimal schemas.
/// </summary>
public static class DecimalSchemaExtensions
{
    public static TSelf Min<TSelf>(this IValueSchema<decimal, TSelf> schema, decimal min, string? message = null)
        where TSelf : IValueSchema<decimal, TSelf>
        => schema.AppendRule(new MinDecimalRule(min, message));

    public static TSelf Max<TSelf>(this IValueSchema<decimal, TSelf> schema, decimal max, string? message = null)
        where TSelf : IValueSchema<decimal, TSelf>
        => schema.AppendRule(new MaxDecimalRule(max, message));

    public static TSelf Range<TSelf>(this IValueSchema<decimal, TSelf> schema, decimal min, decimal max, string? message = null)
        where TSelf : IValueSchema<decimal, TSelf>
    {
        if (min > max)
            throw new ArgumentOutOfRangeException(nameof(min), "min must be less than or equal to max.");

        return schema.Min(min, message).Max(max, message);
    }

    public static TSelf Positive<TSelf>(this IValueSchema<decimal, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<decimal, TSelf>
        => schema.AppendRule(new PositiveDecimalRule(message));

    public static TSelf Negative<TSelf>(this IValueSchema<decimal, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<decimal, TSelf>
        => schema.AppendRule(new NegativeDecimalRule(message));

    public static TSelf Precision<TSelf>(this IValueSchema<decimal, TSelf> schema, int maxDecimalPlaces, string? message = null)
        where TSelf : IValueSchema<decimal, TSelf>
        => schema.AppendRule(new PrecisionRule(maxDecimalPlaces, message));

    public static TSelf MultipleOf<TSelf>(this IValueSchema<decimal, TSelf> schema, decimal step, string? message = null)
        where TSelf : IValueSchema<decimal, TSelf>
        => schema.AppendRule(new MultipleOfRule(step, message));
}
