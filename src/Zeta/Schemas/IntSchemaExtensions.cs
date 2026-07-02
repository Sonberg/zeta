using Zeta.Core;
using Zeta.Rules.Numeric;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Integer validators. Work on both contextless and context-aware int schemas.
/// </summary>
public static class IntSchemaExtensions
{
    /// <summary>Requires the value to be greater than or equal to <paramref name="min"/>.</summary>
    public static TSelf Min<TSelf>(this IValueSchema<int, TSelf> schema, int min, string? message = null)
        where TSelf : IValueSchema<int, TSelf>
        => schema.AppendRule(new MinIntRule(min, message));

    /// <summary>Requires the value to be less than or equal to <paramref name="max"/>.</summary>
    public static TSelf Max<TSelf>(this IValueSchema<int, TSelf> schema, int max, string? message = null)
        where TSelf : IValueSchema<int, TSelf>
        => schema.AppendRule(new MaxIntRule(max, message));

    /// <summary>Requires the value to fall within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public static TSelf Range<TSelf>(this IValueSchema<int, TSelf> schema, int min, int max, string? message = null)
        where TSelf : IValueSchema<int, TSelf>
    {
        if (min > max)
            throw new ArgumentOutOfRangeException(nameof(min), "min must be less than or equal to max.");

        return schema.Min(min, message).Max(max, message);
    }

    /// <summary>Requires the value to be positive (greater than 0).</summary>
    public static TSelf Positive<TSelf>(this IValueSchema<int, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<int, TSelf>
        => schema.AppendRule(new PositiveIntRule(message));

    /// <summary>Requires the value to be negative (less than 0).</summary>
    public static TSelf Negative<TSelf>(this IValueSchema<int, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<int, TSelf>
        => schema.AppendRule(new NegativeIntRule(message));

    /// <summary>Requires the value to be a multiple of <paramref name="step"/>.</summary>
    public static TSelf MultipleOf<TSelf>(this IValueSchema<int, TSelf> schema, int step, string? message = null)
        where TSelf : IValueSchema<int, TSelf>
        => schema.AppendRule(new MultipleOfIntRule(step, message));
}
