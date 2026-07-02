using Zeta.Core;
using Zeta.Rules;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Boolean validators. Work on both contextless and context-aware bool schemas.
/// </summary>
public static class BoolSchemaExtensions
{
    /// <summary>Requires the value to be true.</summary>
    public static TSelf IsTrue<TSelf>(this IValueSchema<bool, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<bool, TSelf>
        => schema.AppendRule(new IsTrueRule(message));

    /// <summary>Requires the value to be false.</summary>
    public static TSelf IsFalse<TSelf>(this IValueSchema<bool, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<bool, TSelf>
        => schema.AppendRule(new IsFalseRule(message));
}
