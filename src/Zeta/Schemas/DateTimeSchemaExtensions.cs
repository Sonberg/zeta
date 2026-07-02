using Zeta.Core;
using Zeta.Rules;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// DateTime validators. Work on both contextless and context-aware DateTime schemas.
/// </summary>
public static class DateTimeSchemaExtensions
{
    /// <summary>Requires the value to be at or after <paramref name="min"/>.</summary>
    public static TSelf Min<TSelf>(this IValueSchema<DateTime, TSelf> schema, DateTime min, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new DateTimeMinRule(min, message));

    /// <summary>Requires the value to be at or before <paramref name="max"/>.</summary>
    public static TSelf Max<TSelf>(this IValueSchema<DateTime, TSelf> schema, DateTime max, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new DateTimeMaxRule(max, message));

    /// <summary>Requires the value to be earlier than the current time.</summary>
    public static TSelf Past<TSelf>(this IValueSchema<DateTime, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new DateTimePastRule(message));

    /// <summary>Requires the value to be later than the current time.</summary>
    public static TSelf Future<TSelf>(this IValueSchema<DateTime, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new DateTimeFutureRule(message));

    /// <summary>Requires the value to fall within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public static TSelf Between<TSelf>(this IValueSchema<DateTime, TSelf> schema, DateTime min, DateTime max, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new DateTimeBetweenRule(min, max, message));

    /// <summary>Requires the value to fall on a Monday through Friday.</summary>
    public static TSelf Weekday<TSelf>(this IValueSchema<DateTime, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new DateTimeWeekdayRule(message));

    /// <summary>Requires the value to fall on a Saturday or Sunday.</summary>
    public static TSelf Weekend<TSelf>(this IValueSchema<DateTime, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new DateTimeWeekendRule(message));

    /// <summary>Requires the value to be within <paramref name="days"/> days of the current time, in either direction.</summary>
    public static TSelf WithinDays<TSelf>(this IValueSchema<DateTime, TSelf> schema, int days, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new DateTimeWithinDaysRule(days, message));

    /// <summary>Requires the value, interpreted as a birth date, to represent an age of at least <paramref name="years"/>.</summary>
    public static TSelf MinAge<TSelf>(this IValueSchema<DateTime, TSelf> schema, int years, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new DateTimeMinAgeRule(years, message));

    /// <summary>Requires the value, interpreted as a birth date, to represent an age of at most <paramref name="years"/>.</summary>
    public static TSelf MaxAge<TSelf>(this IValueSchema<DateTime, TSelf> schema, int years, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new DateTimeMaxAgeRule(years, message));
}
