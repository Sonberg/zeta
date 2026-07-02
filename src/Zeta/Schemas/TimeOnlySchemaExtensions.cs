using Zeta.Core;
using Zeta.Rules;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// TimeOnly validators. Work on both contextless and context-aware TimeOnly schemas.
/// </summary>
public static class TimeOnlySchemaExtensions
{
    /// <summary>Requires the value to be at or after <paramref name="min"/>.</summary>
    public static TSelf Min<TSelf>(this IValueSchema<TimeOnly, TSelf> schema, TimeOnly min, string? message = null)
        where TSelf : IValueSchema<TimeOnly, TSelf>
        => schema.AppendRule(new TimeOnlyMinRule(min, message));

    /// <summary>Requires the value to be at or before <paramref name="max"/>.</summary>
    public static TSelf Max<TSelf>(this IValueSchema<TimeOnly, TSelf> schema, TimeOnly max, string? message = null)
        where TSelf : IValueSchema<TimeOnly, TSelf>
        => schema.AppendRule(new TimeOnlyMaxRule(max, message));

    /// <summary>Requires the value to fall within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public static TSelf Between<TSelf>(this IValueSchema<TimeOnly, TSelf> schema, TimeOnly min, TimeOnly max, string? message = null)
        where TSelf : IValueSchema<TimeOnly, TSelf>
        => schema.AppendRule(new TimeOnlyBetweenRule(min, max, message));

    /// <summary>Requires the value to fall within business hours (defaults to 09:00-17:00).</summary>
    public static TSelf BusinessHours<TSelf>(this IValueSchema<TimeOnly, TSelf> schema, TimeOnly? start = null, TimeOnly? end = null, string? message = null)
        where TSelf : IValueSchema<TimeOnly, TSelf>
    {
        var businessStart = start ?? new TimeOnly(9, 0);
        var businessEnd = end ?? new TimeOnly(17, 0);
        return schema.AppendRule(new TimeOnlyBusinessHoursRule(businessStart, businessEnd, message));
    }

    /// <summary>Requires the value to be before 12:00.</summary>
    public static TSelf Morning<TSelf>(this IValueSchema<TimeOnly, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<TimeOnly, TSelf>
        => schema.AppendRule(new TimeOnlyMorningRule(message));

    /// <summary>Requires the value to be between 12:00 and 18:00.</summary>
    public static TSelf Afternoon<TSelf>(this IValueSchema<TimeOnly, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<TimeOnly, TSelf>
        => schema.AppendRule(new TimeOnlyAfternoonRule(message));

    /// <summary>Requires the value to be at or after 18:00.</summary>
    public static TSelf Evening<TSelf>(this IValueSchema<TimeOnly, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<TimeOnly, TSelf>
        => schema.AppendRule(new TimeOnlyEveningRule(message));
}
