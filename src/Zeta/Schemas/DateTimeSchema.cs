using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating DateTime values.
/// </summary>
public sealed class DateTimeSchema : ContextlessSchema<DateTime>
{
    public DateTimeSchema() { }

    public DateTimeSchema Min(DateTime min, string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
            val >= min
                ? null
                : new ValidationError(exec.Path, "min_date", message ?? $"Must be at or after {min:O}")));
        return this;
    }

    public DateTimeSchema Max(DateTime max, string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
            val <= max
                ? null
                : new ValidationError(exec.Path, "max_date", message ?? $"Must be at or before {max:O}")));
        return this;
    }

    public DateTimeSchema Past(string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
            val < exec.TimeProvider.GetUtcNow().UtcDateTime
                ? null
                : new ValidationError(exec.Path, "past", message ?? "Must be in the past")));
        return this;
    }

    public DateTimeSchema Future(string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
            val > exec.TimeProvider.GetUtcNow().UtcDateTime
                ? null
                : new ValidationError(exec.Path, "future", message ?? "Must be in the future")));
        return this;
    }

    public DateTimeSchema Between(DateTime min, DateTime max, string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
            val >= min && val <= max
                ? null
                : new ValidationError(exec.Path, "between", message ?? $"Must be between {min:O} and {max:O}")));
        return this;
    }

    public DateTimeSchema Weekday(string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday
                ? null
                : new ValidationError(exec.Path, "weekday", message ?? "Must be a weekday")));
        return this;
    }

    public DateTimeSchema Weekend(string? message = null)
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday
                ? null
                : new ValidationError(exec.Path, "weekend", message ?? "Must be a weekend")));
        return this;
    }

    public DateTimeSchema WithinDays(int days, string? message = null)
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

    public DateTimeSchema MinAge(int years, string? message = null)
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

    public DateTimeSchema MaxAge(int years, string? message = null)
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

    public DateTimeSchema Refine(Func<DateTime, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<DateTime>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    /// <summary>
    /// Creates a context-aware DateTime schema with all rules from this schema.
    /// </summary>
    public DateTimeSchema<TContext> WithContext<TContext>()
        => new DateTimeSchema<TContext>(Rules.ToContext<TContext>());
}

/// <summary>
/// A context-aware schema for validating DateTime values.
/// </summary>
public class DateTimeSchema<TContext> : ContextSchema<DateTime, TContext>
{
    public DateTimeSchema() { }

    public DateTimeSchema(ContextRuleEngine<DateTime, TContext> rules) : base(rules) { }

    public DateTimeSchema<TContext> Min(DateTime min, string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val >= min
                ? null
                : new ValidationError(ctx.Execution.Path, "min_date", message ?? $"Must be at or after {min:O}")));
        return this;
    }

    public DateTimeSchema<TContext> Max(DateTime max, string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val <= max
                ? null
                : new ValidationError(ctx.Execution.Path, "max_date", message ?? $"Must be at or before {max:O}")));
        return this;
    }

    public DateTimeSchema<TContext> Past(string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val < ctx.Execution.TimeProvider.GetUtcNow().UtcDateTime
                ? null
                : new ValidationError(ctx.Execution.Path, "past", message ?? "Must be in the past")));
        return this;
    }

    public DateTimeSchema<TContext> Future(string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val > ctx.Execution.TimeProvider.GetUtcNow().UtcDateTime
                ? null
                : new ValidationError(ctx.Execution.Path, "future", message ?? "Must be in the future")));
        return this;
    }

    public DateTimeSchema<TContext> Between(DateTime min, DateTime max, string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val >= min && val <= max
                ? null
                : new ValidationError(ctx.Execution.Path, "between", message ?? $"Must be between {min:O} and {max:O}")));
        return this;
    }

    public DateTimeSchema<TContext> Weekday(string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday
                ? null
                : new ValidationError(ctx.Execution.Path, "weekday", message ?? "Must be a weekday")));
        return this;
    }

    public DateTimeSchema<TContext> Weekend(string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday
                ? null
                : new ValidationError(ctx.Execution.Path, "weekend", message ?? "Must be a weekend")));
        return this;
    }

    public DateTimeSchema<TContext> WithinDays(int days, string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
        {
            var now = ctx.Execution.TimeProvider.GetUtcNow().UtcDateTime;
            var diff = Math.Abs((val - now).TotalDays);
            return diff <= days
                ? null
                : new ValidationError(ctx.Execution.Path, "within_days", message ?? $"Must be within {days} days from now");
        }));
        return this;
    }

    public DateTimeSchema<TContext> MinAge(int years, string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
        {
            var today = ctx.Execution.TimeProvider.GetUtcNow().UtcDateTime.Date;
            var age = today.Year - val.Year;
            if (val.Date > today.AddYears(-age)) age--;

            return age >= years
                ? null
                : new ValidationError(ctx.Execution.Path, "min_age", message ?? $"Must be at least {years} years old");
        }));
        return this;
    }

    public DateTimeSchema<TContext> MaxAge(int years, string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
        {
            var today = ctx.Execution.TimeProvider.GetUtcNow().UtcDateTime.Date;
            var age = today.Year - val.Year;
            if (val.Date > today.AddYears(-age)) age--;

            return age <= years
                ? null
                : new ValidationError(ctx.Execution.Path, "max_age", message ?? $"Must be at most {years} years old");
        }));
        return this;
    }

    public DateTimeSchema<TContext> Refine(Func<DateTime, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
    }

    public DateTimeSchema<TContext> Refine(Func<DateTime, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}
