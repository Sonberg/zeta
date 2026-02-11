using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating DateTime values.
/// </summary>
public sealed class DateTimeContextlessSchema : ContextlessSchema<DateTime, DateTimeContextlessSchema>
{
    internal DateTimeContextlessSchema()
    {
    }

    protected override DateTimeContextlessSchema CreateInstance() => new();

    public DateTimeContextlessSchema Min(DateTime min, string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
            val >= min
                ? null
                : new ValidationError(exec.Path, "min_date", message ?? $"Must be at or after {min:O}")));
        return this;
    }

    public DateTimeContextlessSchema Max(DateTime max, string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
            val <= max
                ? null
                : new ValidationError(exec.Path, "max_date", message ?? $"Must be at or before {max:O}")));
        return this;
    }

    public DateTimeContextlessSchema Past(string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
            val < exec.TimeProvider.GetUtcNow().UtcDateTime
                ? null
                : new ValidationError(exec.Path, "past", message ?? "Must be in the past")));
        return this;
    }

    public DateTimeContextlessSchema Future(string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
            val > exec.TimeProvider.GetUtcNow().UtcDateTime
                ? null
                : new ValidationError(exec.Path, "future", message ?? "Must be in the future")));
        return this;
    }

    public DateTimeContextlessSchema Between(DateTime min, DateTime max, string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
            val >= min && val <= max
                ? null
                : new ValidationError(exec.Path, "between", message ?? $"Must be between {min:O} and {max:O}")));
        return this;
    }

    public DateTimeContextlessSchema Weekday(string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday
                ? null
                : new ValidationError(exec.Path, "weekday", message ?? "Must be a weekday")));
        return this;
    }

    public DateTimeContextlessSchema Weekend(string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday
                ? null
                : new ValidationError(exec.Path, "weekend", message ?? "Must be a weekend")));
        return this;
    }

    public DateTimeContextlessSchema WithinDays(int days, string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
        {
            var now = exec.TimeProvider.GetUtcNow().UtcDateTime;
            var diff = Math.Abs((val - now).TotalDays);
            return diff <= days
                ? null
                : new ValidationError(exec.Path, "within_days", message ?? $"Must be within {days} days from now");
        }));
        return this;
    }

    public DateTimeContextlessSchema MinAge(int years, string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
        {
            var today = exec.TimeProvider.GetUtcNow().UtcDateTime.Date;
            var age = today.Year - val.Year;
            if (val.Date > today.AddYears(-age)) age--;

            return age >= years
                ? null
                : new ValidationError(exec.Path, "min_age", message ?? $"Must be at least {years} years old");
        }));
        return this;
    }

    public DateTimeContextlessSchema MaxAge(int years, string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
        {
            var today = exec.TimeProvider.GetUtcNow().UtcDateTime.Date;
            var age = today.Year - val.Year;
            if (val.Date > today.AddYears(-age)) age--;

            return age <= years
                ? null
                : new ValidationError(exec.Path, "max_age", message ?? $"Must be at most {years} years old");
        }));
        return this;
    }

    /// <summary>
    /// Creates a context-aware DateTime schema with all rules from this schema.
    /// </summary>
    public DateTimeContextSchema<TContext> WithContext<TContext>()
    {
        var schema = new DateTimeContextSchema<TContext>(Rules.ToContext<TContext>());
        if (AllowNull) schema.Nullable();
        schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }
}