#if !NETSTANDARD2_0
using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating DateOnly values.
/// </summary>
public class DateOnlyContextSchema<TContext> : ContextSchema<DateOnly, TContext>
{
    public DateOnlyContextSchema() { }

    public DateOnlyContextSchema(ContextRuleEngine<DateOnly, TContext> rules) : base(rules) { }

    public DateOnlyContextSchema<TContext> Min(DateOnly min, string? message = null)
    {
        Use(new RefinementRule<DateOnly, TContext>((val, ctx) =>
            val >= min
                ? null
                : new ValidationError(ctx.Path, "min_date", message ?? $"Must be at or after {min:O}")));
        return this;
    }

    public DateOnlyContextSchema<TContext> Max(DateOnly max, string? message = null)
    {
        Use(new RefinementRule<DateOnly, TContext>((val, ctx) =>
            val <= max
                ? null
                : new ValidationError(ctx.Path, "max_date", message ?? $"Must be at or before {max:O}")));
        return this;
    }

    public DateOnlyContextSchema<TContext> Past(string? message = null)
    {
        Use(new RefinementRule<DateOnly, TContext>((val, ctx) =>
        {
            var today = DateOnly.FromDateTime(ctx.TimeProvider.GetUtcNow().UtcDateTime);
            return val < today
                ? null
                : new ValidationError(ctx.Path, "past", message ?? "Must be in the past");
        }));
        return this;
    }

    public DateOnlyContextSchema<TContext> Future(string? message = null)
    {
        Use(new RefinementRule<DateOnly, TContext>((val, ctx) =>
        {
            var today = DateOnly.FromDateTime(ctx.TimeProvider.GetUtcNow().UtcDateTime);
            return val > today
                ? null
                : new ValidationError(ctx.Path, "future", message ?? "Must be in the future");
        }));
        return this;
    }

    public DateOnlyContextSchema<TContext> Between(DateOnly min, DateOnly max, string? message = null)
    {
        Use(new RefinementRule<DateOnly, TContext>((val, ctx) =>
            val >= min && val <= max
                ? null
                : new ValidationError(ctx.Path, "between", message ?? $"Must be between {min:O} and {max:O}")));
        return this;
    }

    public DateOnlyContextSchema<TContext> Weekday(string? message = null)
    {
        Use(new RefinementRule<DateOnly, TContext>((val, ctx) =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday
                ? null
                : new ValidationError(ctx.Path, "weekday", message ?? "Must be a weekday")));
        return this;
    }

    public DateOnlyContextSchema<TContext> Weekend(string? message = null)
    {
        Use(new RefinementRule<DateOnly, TContext>((val, ctx) =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday
                ? null
                : new ValidationError(ctx.Path, "weekend", message ?? "Must be a weekend")));
        return this;
    }

    public DateOnlyContextSchema<TContext> MinAge(int years, string? message = null)
    {
        Use(new RefinementRule<DateOnly, TContext>((val, ctx) =>
        {
            var today = DateOnly.FromDateTime(ctx.TimeProvider.GetUtcNow().UtcDateTime);
            var age = today.Year - val.Year;
            if (val > today.AddYears(-age)) age--;

            return age >= years
                ? null
                : new ValidationError(ctx.Path, "min_age", message ?? $"Must be at least {years} years old");
        }));
        return this;
    }

    public DateOnlyContextSchema<TContext> MaxAge(int years, string? message = null)
    {
        Use(new RefinementRule<DateOnly, TContext>((val, ctx) =>
        {
            var today = DateOnly.FromDateTime(ctx.TimeProvider.GetUtcNow().UtcDateTime);
            var age = today.Year - val.Year;
            if (val > today.AddYears(-age)) age--;

            return age <= years
                ? null
                : new ValidationError(ctx.Path, "max_age", message ?? $"Must be at most {years} years old");
        }));
        return this;
    }

    public DateOnlyContextSchema<TContext> Refine(Func<DateOnly, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<DateOnly, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public DateOnlyContextSchema<TContext> Refine(Func<DateOnly, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }

    public DateOnlyContextSchema<TContext> RefineAsync(
        Func<DateOnly, TContext, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        Use(new RefinementRule<DateOnly, TContext>(async (val, ctx) =>
            await predicate(val, ctx.Data, ctx.CancellationToken)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public DateOnlyContextSchema<TContext> RefineAsync(
        Func<DateOnly, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        return RefineAsync((val, _, ct) => predicate(val, ct), message, code);
    }

    public DateOnlyContextSchema<TContext> If(
        Func<DateOnly, TContext, bool> condition,
        Func<DateOnlyContextSchema<TContext>, DateOnlyContextSchema<TContext>> configure)
    {
        var inner = configure(new DateOnlyContextSchema<TContext>());
        foreach (var rule in inner.Rules.GetRules())
            Use(new ConditionalRule<DateOnly, TContext>(condition, rule));
        return this;
    }

    public DateOnlyContextSchema<TContext> If(
        Func<DateOnly, bool> condition,
        Func<DateOnlyContextSchema<TContext>, DateOnlyContextSchema<TContext>> configure)
        => If((val, _) => condition(val), configure);
}
#endif
