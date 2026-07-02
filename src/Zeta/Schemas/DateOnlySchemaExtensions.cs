using Zeta.Core;
using Zeta.Rules;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// DateOnly validators. Work on both contextless and context-aware DateOnly schemas.
/// </summary>
public static class DateOnlySchemaExtensions
{
    public static TSelf Min<TSelf>(this IValueSchema<DateOnly, TSelf> schema, DateOnly min, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new RefinementRule<DateOnly>((val, exec) =>
            val >= min
                ? null
                : new ValidationError(exec.PathSegments, "min_date", message ?? $"Must be at or after {min:O}")));

    public static TSelf Max<TSelf>(this IValueSchema<DateOnly, TSelf> schema, DateOnly max, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new RefinementRule<DateOnly>((val, exec) =>
            val <= max
                ? null
                : new ValidationError(exec.PathSegments, "max_date", message ?? $"Must be at or before {max:O}")));

    public static TSelf Past<TSelf>(this IValueSchema<DateOnly, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new RefinementRule<DateOnly>((val, exec) =>
        {
            var today = DateOnly.FromDateTime(exec.TimeProvider.GetUtcNow().UtcDateTime);
            return val < today
                ? null
                : new ValidationError(exec.PathSegments, "past", message ?? "Must be in the past");
        }));

    public static TSelf Future<TSelf>(this IValueSchema<DateOnly, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new RefinementRule<DateOnly>((val, exec) =>
        {
            var today = DateOnly.FromDateTime(exec.TimeProvider.GetUtcNow().UtcDateTime);
            return val > today
                ? null
                : new ValidationError(exec.PathSegments, "future", message ?? "Must be in the future");
        }));

    public static TSelf Between<TSelf>(this IValueSchema<DateOnly, TSelf> schema, DateOnly min, DateOnly max, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new RefinementRule<DateOnly>((val, exec) =>
            val >= min && val <= max
                ? null
                : new ValidationError(exec.PathSegments, "between", message ?? $"Must be between {min:O} and {max:O}")));

    public static TSelf Weekday<TSelf>(this IValueSchema<DateOnly, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new RefinementRule<DateOnly>((val, exec) =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday
                ? null
                : new ValidationError(exec.PathSegments, "weekday", message ?? "Must be a weekday")));

    public static TSelf Weekend<TSelf>(this IValueSchema<DateOnly, TSelf> schema, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new RefinementRule<DateOnly>((val, exec) =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday
                ? null
                : new ValidationError(exec.PathSegments, "weekend", message ?? "Must be a weekend")));

    public static TSelf MinAge<TSelf>(this IValueSchema<DateOnly, TSelf> schema, int years, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new RefinementRule<DateOnly>((val, exec) =>
        {
            var today = DateOnly.FromDateTime(exec.TimeProvider.GetUtcNow().UtcDateTime);
            var age = today.Year - val.Year;
            if (val > today.AddYears(-age)) age--;

            return age >= years
                ? null
                : new ValidationError(exec.PathSegments, "min_age", message ?? $"Must be at least {years} years old");
        }));

    public static TSelf MaxAge<TSelf>(this IValueSchema<DateOnly, TSelf> schema, int years, string? message = null)
        where TSelf : IValueSchema<DateOnly, TSelf>
        => schema.AppendRule(new RefinementRule<DateOnly>((val, exec) =>
        {
            var today = DateOnly.FromDateTime(exec.TimeProvider.GetUtcNow().UtcDateTime);
            var age = today.Year - val.Year;
            if (val > today.AddYears(-age)) age--;

            return age <= years
                ? null
                : new ValidationError(exec.PathSegments, "max_age", message ?? $"Must be at most {years} years old");
        }));
}
