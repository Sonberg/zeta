using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating DateTime values.
/// </summary>
public class DateTimeContextSchema<TContext> : ContextSchema<DateTime, TContext>
{
    public DateTimeContextSchema() { }

    public DateTimeContextSchema(ContextRuleEngine<DateTime, TContext> rules) : base(rules) { }

    public DateTimeContextSchema<TContext> Min(DateTime min, string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val >= min
                ? null
                : new ValidationError(ctx.Path, "min_date", message ?? $"Must be at or after {min:O}")));
        return this;
    }

    public DateTimeContextSchema<TContext> Max(DateTime max, string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val <= max
                ? null
                : new ValidationError(ctx.Path, "max_date", message ?? $"Must be at or before {max:O}")));
        return this;
    }

    public DateTimeContextSchema<TContext> Past(string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val < ctx.TimeProvider.GetUtcNow().UtcDateTime
                ? null
                : new ValidationError(ctx.Path, "past", message ?? "Must be in the past")));
        return this;
    }

    public DateTimeContextSchema<TContext> Future(string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val > ctx.TimeProvider.GetUtcNow().UtcDateTime
                ? null
                : new ValidationError(ctx.Path, "future", message ?? "Must be in the future")));
        return this;
    }

    public DateTimeContextSchema<TContext> Between(DateTime min, DateTime max, string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val >= min && val <= max
                ? null
                : new ValidationError(ctx.Path, "between", message ?? $"Must be between {min:O} and {max:O}")));
        return this;
    }

    public DateTimeContextSchema<TContext> Weekday(string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday
                ? null
                : new ValidationError(ctx.Path, "weekday", message ?? "Must be a weekday")));
        return this;
    }

    public DateTimeContextSchema<TContext> Weekend(string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday
                ? null
                : new ValidationError(ctx.Path, "weekend", message ?? "Must be a weekend")));
        return this;
    }

    public DateTimeContextSchema<TContext> WithinDays(int days, string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
        {
            var now = ctx.TimeProvider.GetUtcNow().UtcDateTime;
            var diff = Math.Abs((val - now).TotalDays);
            return diff <= days
                ? null
                : new ValidationError(ctx.Path, "within_days", message ?? $"Must be within {days} days from now");
        }));
        return this;
    }

    public DateTimeContextSchema<TContext> MinAge(int years, string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
        {
            var today = ctx.TimeProvider.GetUtcNow().UtcDateTime.Date;
            var age = today.Year - val.Year;
            if (val.Date > today.AddYears(-age)) age--;

            return age >= years
                ? null
                : new ValidationError(ctx.Path, "min_age", message ?? $"Must be at least {years} years old");
        }));
        return this;
    }

    public DateTimeContextSchema<TContext> MaxAge(int years, string? message = null)
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
        {
            var today = ctx.TimeProvider.GetUtcNow().UtcDateTime.Date;
            var age = today.Year - val.Year;
            if (val.Date > today.AddYears(-age)) age--;

            return age <= years
                ? null
                : new ValidationError(ctx.Path, "max_age", message ?? $"Must be at most {years} years old");
        }));
        return this;
    }

    public DateTimeContextSchema<TContext> Refine(Func<DateTime, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<DateTime, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public DateTimeContextSchema<TContext> Refine(Func<DateTime, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }

    public DateTimeContextSchema<TContext> RefineAsync(
        Func<DateTime, TContext, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        Use(new RefinementRule<DateTime, TContext>(async (val, ctx) =>
            await predicate(val, ctx.Data, ctx.CancellationToken)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public DateTimeContextSchema<TContext> RefineAsync(
        Func<DateTime, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        return RefineAsync((val, _, ct) => predicate(val, ct), message, code);
    }

    public DateTimeContextSchema<TContext> If(
        Func<DateTime, TContext, bool> condition,
        Func<DateTimeContextSchema<TContext>, DateTimeContextSchema<TContext>> configure)
    {
        var inner = configure(new DateTimeContextSchema<TContext>());
        foreach (var rule in inner.Rules.GetRules())
            Use(new ConditionalRule<DateTime, TContext>(condition, rule));
        return this;
    }

    public DateTimeContextSchema<TContext> If(
        Func<DateTime, bool> condition,
        Func<DateTimeContextSchema<TContext>, DateTimeContextSchema<TContext>> configure)
        => If((val, _) => condition(val), configure);
}
