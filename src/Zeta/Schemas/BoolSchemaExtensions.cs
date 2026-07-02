using Zeta.Core;
using Zeta.Rules;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Boolean validators. Work on both contextless and context-aware bool schemas.
/// </summary>
public static class BoolSchemaExtensions
{
    public static TSelf IsTrue<TSelf>(this IValueSchema<bool, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<bool, TSelf>
        => schema.AppendRule(new RefinementRule<bool>((val, exec) =>
            val
                ? null
                : new ValidationError(exec.PathSegments, "is_true", message ?? "Must be true")));

    public static TSelf IsFalse<TSelf>(this IValueSchema<bool, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<bool, TSelf>
        => schema.AppendRule(new RefinementRule<bool>((val, exec) =>
            !val
                ? null
                : new ValidationError(exec.PathSegments, "is_false", message ?? "Must be false")));
}
