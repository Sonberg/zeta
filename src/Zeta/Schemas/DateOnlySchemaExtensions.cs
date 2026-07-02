using Zeta.Core;
using Zeta.Rules;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// DateOnly validators. Work on both contextless and context-aware DateOnly schemas.
/// </summary>
public static class DateOnlySchemaExtensions
{
    /// <summary>Requires the value to be at or after <paramref name="min"/>.</summary>
    public static TSelf Min<TSelf>(this IValueSchema<DateOnly, TSelf> schema, DateOnly min, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new DateOnlyMinRule(min, message));

    /// <summary>Requires the value to be at or before <paramref name="max"/>.</summary>
    public static TSelf Max<TSelf>(this IValueSchema<DateOnly, TSelf> schema, DateOnly max, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new DateOnlyMaxRule(max, message));

    /// <summary>Requires the value to be earlier than today.</summary>
    public static TSelf Past<TSelf>(this IValueSchema<DateOnly, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new DateOnlyPastRule(message));

    /// <summary>Requires the value to be later than today.</summary>
    public static TSelf Future<TSelf>(this IValueSchema<DateOnly, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new DateOnlyFutureRule(message));

    /// <summary>Requires the value to fall within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public static TSelf Between<TSelf>(this IValueSchema<DateOnly, TSelf> schema, DateOnly min, DateOnly max, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new DateOnlyBetweenRule(min, max, message));

    /// <summary>Requires the value to fall on a Monday through Friday.</summary>
    public static TSelf Weekday<TSelf>(this IValueSchema<DateOnly, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new DateOnlyWeekdayRule(message));

    /// <summary>Requires the value to fall on a Saturday or Sunday.</summary>
    public static TSelf Weekend<TSelf>(this IValueSchema<DateOnly, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new DateOnlyWeekendRule(message));

    /// <summary>Requires the value, interpreted as a birth date, to represent an age of at least <paramref name="years"/>.</summary>
    public static TSelf MinAge<TSelf>(this IValueSchema<DateOnly, TSelf> schema, int years, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new DateOnlyMinAgeRule(years, message));

    /// <summary>Requires the value, interpreted as a birth date, to represent an age of at most <paramref name="years"/>.</summary>
    public static TSelf MaxAge<TSelf>(this IValueSchema<DateOnly, TSelf> schema, int years, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new DateOnlyMaxAgeRule(years, message));
}
