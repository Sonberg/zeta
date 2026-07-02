using Zeta.Core;
using Zeta.Rules.Numeric;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Double validators. Work on both contextless and context-aware double schemas.
/// </summary>
public static class DoubleSchemaExtensions
{
    public static TSelf Min<TSelf>(this IValueSchema<double, TSelf> schema, double min, string? message = null)
        where TSelf : IValueSchema<double, TSelf>
        => schema.AppendRule(new MinDoubleRule(min, message));

    public static TSelf Max<TSelf>(this IValueSchema<double, TSelf> schema, double max, string? message = null)
        where TSelf : IValueSchema<double, TSelf>
        => schema.AppendRule(new MaxDoubleRule(max, message));

    public static TSelf Range<TSelf>(this IValueSchema<double, TSelf> schema, double min, double max, string? message = null)
        where TSelf : IValueSchema<double, TSelf>
    {
        if (min > max)
            throw new ArgumentOutOfRangeException(nameof(min), "min must be less than or equal to max.");

        return schema.Min(min, message).Max(max, message);
    }

    public static TSelf Positive<TSelf>(this IValueSchema<double, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<double, TSelf>
        => schema.AppendRule(new PositiveDoubleRule(message));

    public static TSelf Negative<TSelf>(this IValueSchema<double, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<double, TSelf>
        => schema.AppendRule(new NegativeDoubleRule(message));

    public static TSelf Finite<TSelf>(this IValueSchema<double, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<double, TSelf>
        => schema.AppendRule(new FiniteRule(message));
}
