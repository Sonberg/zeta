using Zeta.Core;
using Zeta.Rules;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// DateTime validators. Work on both contextless and context-aware DateTime schemas.
/// </summary>
public static class DateTimeSchemaExtensions
{
    public static TSelf Min<TSelf>(this IValueSchema<DateTime, TSelf> schema, DateTime min, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new RefinementRule<DateTime>((val, exec) =>
            val >= min
                ? null
                : new ValidationError(exec.PathSegments, "min_date", message ?? $"Must be at or after {min:O}")));

    public static TSelf Max<TSelf>(this IValueSchema<DateTime, TSelf> schema, DateTime max, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new RefinementRule<DateTime>((val, exec) =>
            val <= max
                ? null
                : new ValidationError(exec.PathSegments, "max_date", message ?? $"Must be at or before {max:O}")));

    public static TSelf Past<TSelf>(this IValueSchema<DateTime, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new RefinementRule<DateTime>((val, exec) =>
            val < exec.TimeProvider.GetUtcNow().UtcDateTime
                ? null
                : new ValidationError(exec.PathSegments, "past", message ?? "Must be in the past")));

    public static TSelf Future<TSelf>(this IValueSchema<DateTime, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new RefinementRule<DateTime>((val, exec) =>
            val > exec.TimeProvider.GetUtcNow().UtcDateTime
                ? null
                : new ValidationError(exec.PathSegments, "future", message ?? "Must be in the future")));

    public static TSelf Between<TSelf>(this IValueSchema<DateTime, TSelf> schema, DateTime min, DateTime max, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new RefinementRule<DateTime>((val, exec) =>
            val >= min && val <= max
                ? null
                : new ValidationError(exec.PathSegments, "between", message ?? $"Must be between {min:O} and {max:O}")));

    public static TSelf Weekday<TSelf>(this IValueSchema<DateTime, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new RefinementRule<DateTime>((val, exec) =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday
                ? null
                : new ValidationError(exec.PathSegments, "weekday", message ?? "Must be a weekday")));

    public static TSelf Weekend<TSelf>(this IValueSchema<DateTime, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new RefinementRule<DateTime>((val, exec) =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday
                ? null
                : new ValidationError(exec.PathSegments, "weekend", message ?? "Must be a weekend")));

    public static TSelf WithinDays<TSelf>(this IValueSchema<DateTime, TSelf> schema, int days, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new RefinementRule<DateTime>((val, exec) =>
        {
            var now = exec.TimeProvider.GetUtcNow().UtcDateTime;
            var diff = Math.Abs((val - now).TotalDays);
            return diff <= days
                ? null
                : new ValidationError(exec.PathSegments, "within_days", message ?? $"Must be within {days} days from now");
        }));

    public static TSelf MinAge<TSelf>(this IValueSchema<DateTime, TSelf> schema, int years, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new RefinementRule<DateTime>((val, exec) =>
        {
            var today = exec.TimeProvider.GetUtcNow().UtcDateTime.Date;
            var age = today.Year - val.Year;
            if (val.Date > today.AddYears(-age)) age--;

            return age >= years
                ? null
                : new ValidationError(exec.PathSegments, "min_age", message ?? $"Must be at least {years} years old");
        }));

    public static TSelf MaxAge<TSelf>(this IValueSchema<DateTime, TSelf> schema, int years, string? message = null)
        where TSelf : IValueSchema<DateTime, TSelf>
        => schema.AppendRule(new RefinementRule<DateTime>((val, exec) =>
        {
            var today = exec.TimeProvider.GetUtcNow().UtcDateTime.Date;
            var age = today.Year - val.Year;
            if (val.Date > today.AddYears(-age)) age--;

            return age <= years
                ? null
                : new ValidationError(exec.PathSegments, "max_age", message ?? $"Must be at most {years} years old");
        }));
}
