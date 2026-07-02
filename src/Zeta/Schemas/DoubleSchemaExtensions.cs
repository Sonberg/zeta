using Zeta.Core;
using Zeta.Rules.Numeric;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Double validators. Work on both contextless and context-aware double schemas.
/// </summary>
public static class DoubleSchemaExtensions
{
    /// <summary>Requires the value to be greater than or equal to <paramref name="min"/>.</summary>
    public static TSelf Min<TSelf>(this IValueSchema<double, TSelf> schema, double min, string? message = null)
        where TSelf : IValueSchema<double, TSelf>
        => schema.AppendRule(new MinDoubleRule(min, message));

    /// <summary>Requires the value to be less than or equal to <paramref name="max"/>.</summary>
    public static TSelf Max<TSelf>(this IValueSchema<double, TSelf> schema, double max, string? message = null)
        where TSelf : IValueSchema<double, TSelf>
        => schema.AppendRule(new MaxDoubleRule(max, message));

    /// <summary>Requires the value to fall within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public static TSelf Range<TSelf>(this IValueSchema<double, TSelf> schema, double min, double max, string? message = null)
        where TSelf : IValueSchema<double, TSelf>
    {
        if (min > max)
            throw new ArgumentOutOfRangeException(nameof(min), "min must be less than or equal to max.");

        return schema.Min(min, message).Max(max, message);
    }

    /// <summary>Requires the value to be positive (greater than 0).</summary>
    public static TSelf Positive<TSelf>(this IValueSchema<double, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<double, TSelf>
        => schema.AppendRule(new PositiveDoubleRule(message));

    /// <summary>Requires the value to be negative (less than 0).</summary>
    public static TSelf Negative<TSelf>(this IValueSchema<double, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<double, TSelf>
        => schema.AppendRule(new NegativeDoubleRule(message));

    /// <summary>Requires the value to be finite (not NaN or Infinity).</summary>
    public static TSelf Finite<TSelf>(this IValueSchema<double, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<double, TSelf>
        => schema.AppendRule(new FiniteRule(message));

    /// <summary>Requires the value to be a multiple of <paramref name="step"/>.</summary>
    public static TSelf MultipleOf<TSelf>(this IValueSchema<double, TSelf> schema, double step, string? message = null)
        where TSelf : IValueSchema<double, TSelf>
        => schema.AppendRule(new MultipleOfDoubleRule(step, message));
}
