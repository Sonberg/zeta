using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating DateTime values.
/// </summary>
public class DateTimeContextSchema<TContext> : ContextSchema<DateTime, TContext, DateTimeContextSchema<TContext>>
{
    internal DateTimeContextSchema() { }

    internal DateTimeContextSchema(ContextRuleEngine<DateTime, TContext> rules) : base(rules)
    {
    }

    private DateTimeContextSchema(
        ContextRuleEngine<DateTime, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<DateTime, TContext>>? conditionals,
        Func<DateTime, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
    }

    protected override DateTimeContextSchema<TContext> CreateInstance() => new();

    protected override DateTimeContextSchema<TContext> CreateInstance(
        ContextRuleEngine<DateTime, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<DateTime, TContext>>? conditionals,
        Func<DateTime, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, allowNull, conditionals, contextFactory);

    public DateTimeContextSchema<TContext> Min(DateTime min, string? message = null)
        => Append(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val >= min
                ? null
                : new ValidationError(ctx.Path, "min_date", message ?? $"Must be at or after {min:O}")));

    public DateTimeContextSchema<TContext> Max(DateTime max, string? message = null)
        => Append(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val <= max
                ? null
                : new ValidationError(ctx.Path, "max_date", message ?? $"Must be at or before {max:O}")));

    public DateTimeContextSchema<TContext> Past(string? message = null)
        => Append(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val < ctx.TimeProvider.GetUtcNow().UtcDateTime
                ? null
                : new ValidationError(ctx.Path, "past", message ?? "Must be in the past")));

    public DateTimeContextSchema<TContext> Future(string? message = null)
        => Append(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val > ctx.TimeProvider.GetUtcNow().UtcDateTime
                ? null
                : new ValidationError(ctx.Path, "future", message ?? "Must be in the future")));

    public DateTimeContextSchema<TContext> Between(DateTime min, DateTime max, string? message = null)
        => Append(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val >= min && val <= max
                ? null
                : new ValidationError(ctx.Path, "between", message ?? $"Must be between {min:O} and {max:O}")));

    public DateTimeContextSchema<TContext> Weekday(string? message = null)
        => Append(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday
                ? null
                : new ValidationError(ctx.Path, "weekday", message ?? "Must be a weekday")));

    public DateTimeContextSchema<TContext> Weekend(string? message = null)
        => Append(new RefinementRule<DateTime, TContext>((val, ctx) =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday
                ? null
                : new ValidationError(ctx.Path, "weekend", message ?? "Must be a weekend")));

    public DateTimeContextSchema<TContext> WithinDays(int days, string? message = null)
        => Append(new RefinementRule<DateTime, TContext>((val, ctx) =>
        {
            var now = ctx.TimeProvider.GetUtcNow().UtcDateTime;
            var diff = Math.Abs((val - now).TotalDays);
            return diff <= days
                ? null
                : new ValidationError(ctx.Path, "within_days", message ?? $"Must be within {days} days from now");
        }));

    public DateTimeContextSchema<TContext> MinAge(int years, string? message = null)
        => Append(new RefinementRule<DateTime, TContext>((val, ctx) =>
        {
            var today = ctx.TimeProvider.GetUtcNow().UtcDateTime.Date;
            var age = today.Year - val.Year;
            if (val.Date > today.AddYears(-age)) age--;

            return age >= years
                ? null
                : new ValidationError(ctx.Path, "min_age", message ?? $"Must be at least {years} years old");
        }));

    public DateTimeContextSchema<TContext> MaxAge(int years, string? message = null)
        => Append(new RefinementRule<DateTime, TContext>((val, ctx) =>
        {
            var today = ctx.TimeProvider.GetUtcNow().UtcDateTime.Date;
            var age = today.Year - val.Year;
            if (val.Date > today.AddYears(-age)) age--;

            return age <= years
                ? null
                : new ValidationError(ctx.Path, "max_age", message ?? $"Must be at most {years} years old");
        }));
}
