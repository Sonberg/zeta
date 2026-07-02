using Zeta.Core;
using Zeta.Rules;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// TimeOnly validators. Work on both contextless and context-aware TimeOnly schemas.
/// </summary>
public static class TimeOnlySchemaExtensions
{
    public static TSelf Min<TSelf>(this IValueSchema<TimeOnly, TSelf> schema, TimeOnly min, string? message = null)
        where TSelf : IValueSchema<TimeOnly, TSelf>
        => schema.AppendRule(new RefinementRule<TimeOnly>((val, exec) =>
            val >= min
                ? null
                : new ValidationError(exec.PathSegments, "min_time", message ?? $"Must be at or after {min:t}")));

    public static TSelf Max<TSelf>(this IValueSchema<TimeOnly, TSelf> schema, TimeOnly max, string? message = null)
        where TSelf : IValueSchema<TimeOnly, TSelf>
        => schema.AppendRule(new RefinementRule<TimeOnly>((val, exec) =>
            val <= max
                ? null
                : new ValidationError(exec.PathSegments, "max_time", message ?? $"Must be at or before {max:t}")));

    public static TSelf Between<TSelf>(this IValueSchema<TimeOnly, TSelf> schema, TimeOnly min, TimeOnly max, string? message = null)
        where TSelf : IValueSchema<TimeOnly, TSelf>
        => schema.AppendRule(new RefinementRule<TimeOnly>((val, exec) =>
            val >= min && val <= max
                ? null
                : new ValidationError(exec.PathSegments, "between", message ?? $"Must be between {min:t} and {max:t}")));

    public static TSelf BusinessHours<TSelf>(this IValueSchema<TimeOnly, TSelf> schema, TimeOnly? start = null, TimeOnly? end = null, string? message = null)
        where TSelf : IValueSchema<TimeOnly, TSelf>
    {
        var businessStart = start ?? new TimeOnly(9, 0);
        var businessEnd = end ?? new TimeOnly(17, 0);
        return schema.AppendRule(new RefinementRule<TimeOnly>((val, exec) =>
            val >= businessStart && val <= businessEnd
                ? null
                : new ValidationError(exec.PathSegments, "business_hours", message ?? $"Must be during business hours ({businessStart:t} - {businessEnd:t})")));
    }

    public static TSelf Morning<TSelf>(this IValueSchema<TimeOnly, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<TimeOnly, TSelf>
        => schema.AppendRule(new RefinementRule<TimeOnly>((val, exec) =>
            val.Hour < 12
                ? null
                : new ValidationError(exec.PathSegments, "morning", message ?? "Must be in the morning (before 12:00)")));

    public static TSelf Afternoon<TSelf>(this IValueSchema<TimeOnly, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<TimeOnly, TSelf>
        => schema.AppendRule(new RefinementRule<TimeOnly>((val, exec) =>
            val.Hour >= 12 && val.Hour < 18
                ? null
                : new ValidationError(exec.PathSegments, "afternoon", message ?? "Must be in the afternoon (12:00 - 18:00)")));

    public static TSelf Evening<TSelf>(this IValueSchema<TimeOnly, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<TimeOnly, TSelf>
        => schema.AppendRule(new RefinementRule<TimeOnly>((val, exec) =>
            val.Hour >= 18
                ? null
                : new ValidationError(exec.PathSegments, "evening", message ?? "Must be in the evening (after 18:00)")));
}
