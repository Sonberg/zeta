using Zeta.Core;
using Zeta.Rules.Numeric;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Integer validators. Work on both contextless and context-aware int schemas.
/// </summary>
public static class IntSchemaExtensions
{
    public static TSelf Min<TSelf>(this IValueSchema<int, TSelf> schema, int min, string? message = null)
        where TSelf : IValueSchema<int, TSelf>
        => schema.AppendRule(new MinIntRule(min, message));

    public static TSelf Max<TSelf>(this IValueSchema<int, TSelf> schema, int max, string? message = null)
        where TSelf : IValueSchema<int, TSelf>
        => schema.AppendRule(new MaxIntRule(max, message));

    public static TSelf Range<TSelf>(this IValueSchema<int, TSelf> schema, int min, int max, string? message = null)
        where TSelf : IValueSchema<int, TSelf>
    {
        if (min > max)
            throw new ArgumentOutOfRangeException(nameof(min), "min must be less than or equal to max.");

        return schema.Min(min, message).Max(max, message);
    }
}
