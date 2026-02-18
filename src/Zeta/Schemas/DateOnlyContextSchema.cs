#if !NETSTANDARD2_0
using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating DateOnly values.
/// </summary>
public class DateOnlyContextSchema<TContext> : ContextSchema<DateOnly, TContext, DateOnlyContextSchema<TContext>>
{
    internal DateOnlyContextSchema() { }

    internal DateOnlyContextSchema(ContextRuleEngine<DateOnly, TContext> rules) : base(rules)
    {
    }

    private DateOnlyContextSchema(
        ContextRuleEngine<DateOnly, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<DateOnly, TContext>>? conditionals,
        Func<DateOnly, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
    }

    protected override DateOnlyContextSchema<TContext> CreateInstance() => new();

    private protected override DateOnlyContextSchema<TContext> CreateInstance(
        ContextRuleEngine<DateOnly, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<DateOnly, TContext>>? conditionals,
        Func<DateOnly, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, allowNull, conditionals, contextFactory);

    public DateOnlyContextSchema<TContext> Min(DateOnly min, string? message = null)
        => Append(new RefinementRule<DateOnly, TContext>((val, ctx) =>
            val >= min
                ? null
                : new ValidationError(ctx.Path, "min_date", message ?? $"Must be at or after {min:O}")));

    public DateOnlyContextSchema<TContext> Max(DateOnly max, string? message = null)
        => Append(new RefinementRule<DateOnly, TContext>((val, ctx) =>
            val <= max
                ? null
                : new ValidationError(ctx.Path, "max_date", message ?? $"Must be at or before {max:O}")));

    public DateOnlyContextSchema<TContext> Past(string? message = null)
        => Append(new RefinementRule<DateOnly, TContext>((val, ctx) =>
        {
            var today = DateOnly.FromDateTime(ctx.TimeProvider.GetUtcNow().UtcDateTime);
            return val < today
                ? null
                : new ValidationError(ctx.Path, "past", message ?? "Must be in the past");
        }));

    public DateOnlyContextSchema<TContext> Future(string? message = null)
        => Append(new RefinementRule<DateOnly, TContext>((val, ctx) =>
        {
            var today = DateOnly.FromDateTime(ctx.TimeProvider.GetUtcNow().UtcDateTime);
            return val > today
                ? null
                : new ValidationError(ctx.Path, "future", message ?? "Must be in the future");
        }));

    public DateOnlyContextSchema<TContext> Between(DateOnly min, DateOnly max, string? message = null)
        => Append(new RefinementRule<DateOnly, TContext>((val, ctx) =>
            val >= min && val <= max
                ? null
                : new ValidationError(ctx.Path, "between", message ?? $"Must be between {min:O} and {max:O}")));

    public DateOnlyContextSchema<TContext> Weekday(string? message = null)
        => Append(new RefinementRule<DateOnly, TContext>((val, ctx) =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday
                ? null
                : new ValidationError(ctx.Path, "weekday", message ?? "Must be a weekday")));

    public DateOnlyContextSchema<TContext> Weekend(string? message = null)
        => Append(new RefinementRule<DateOnly, TContext>((val, ctx) =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday
                ? null
                : new ValidationError(ctx.Path, "weekend", message ?? "Must be a weekend")));

    public DateOnlyContextSchema<TContext> MinAge(int years, string? message = null)
        => Append(new RefinementRule<DateOnly, TContext>((val, ctx) =>
        {
            var today = DateOnly.FromDateTime(ctx.TimeProvider.GetUtcNow().UtcDateTime);
            var age = today.Year - val.Year;
            if (val > today.AddYears(-age)) age--;

            return age >= years
                ? null
                : new ValidationError(ctx.Path, "min_age", message ?? $"Must be at least {years} years old");
        }));

    public DateOnlyContextSchema<TContext> MaxAge(int years, string? message = null)
        => Append(new RefinementRule<DateOnly, TContext>((val, ctx) =>
        {
            var today = DateOnly.FromDateTime(ctx.TimeProvider.GetUtcNow().UtcDateTime);
            var age = today.Year - val.Year;
            if (val > today.AddYears(-age)) age--;

            return age <= years
                ? null
                : new ValidationError(ctx.Path, "max_age", message ?? $"Must be at most {years} years old");
        }));
}
#endif
