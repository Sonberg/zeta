#if !NETSTANDARD2_0
using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating TimeOnly values.
/// </summary>
public class TimeOnlyContextSchema<TContext> : ContextSchema<TimeOnly, TContext, TimeOnlyContextSchema<TContext>>
{
    internal TimeOnlyContextSchema() { }

    internal TimeOnlyContextSchema(ContextRuleEngine<TimeOnly, TContext> rules) : base(rules)
    {
    }

    private TimeOnlyContextSchema(
        ContextRuleEngine<TimeOnly, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<TimeOnly, TContext>>? conditionals,
        Func<TimeOnly, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
    }

    protected override TimeOnlyContextSchema<TContext> CreateInstance() => new();

    private protected override TimeOnlyContextSchema<TContext> CreateInstance(
        ContextRuleEngine<TimeOnly, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<TimeOnly, TContext>>? conditionals,
        Func<TimeOnly, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, allowNull, conditionals, contextFactory);

    public TimeOnlyContextSchema<TContext> Min(TimeOnly min, string? message = null)
        => Append(new RefinementRule<TimeOnly, TContext>((val, ctx) =>
            val >= min
                ? null
                : new ValidationError(ctx.Path, "min_time", message ?? $"Must be at or after {min:t}")));

    public TimeOnlyContextSchema<TContext> Max(TimeOnly max, string? message = null)
        => Append(new RefinementRule<TimeOnly, TContext>((val, ctx) =>
            val <= max
                ? null
                : new ValidationError(ctx.Path, "max_time", message ?? $"Must be at or before {max:t}")));

    public TimeOnlyContextSchema<TContext> Between(TimeOnly min, TimeOnly max, string? message = null)
        => Append(new RefinementRule<TimeOnly, TContext>((val, ctx) =>
            val >= min && val <= max
                ? null
                : new ValidationError(ctx.Path, "between", message ?? $"Must be between {min:t} and {max:t}")));

    public TimeOnlyContextSchema<TContext> BusinessHours(TimeOnly? start = null, TimeOnly? end = null, string? message = null)
    {
        var businessStart = start ?? new TimeOnly(9, 0);
        var businessEnd = end ?? new TimeOnly(17, 0);
        return Append(new RefinementRule<TimeOnly, TContext>((val, ctx) =>
            val >= businessStart && val <= businessEnd
                ? null
                : new ValidationError(ctx.Path, "business_hours", message ?? $"Must be during business hours ({businessStart:t} - {businessEnd:t})")));
    }

    public TimeOnlyContextSchema<TContext> Morning(string? message = null)
        => Append(new RefinementRule<TimeOnly, TContext>((val, ctx) =>
            val.Hour < 12
                ? null
                : new ValidationError(ctx.Path, "morning", message ?? "Must be in the morning (before 12:00)")));

    public TimeOnlyContextSchema<TContext> Afternoon(string? message = null)
        => Append(new RefinementRule<TimeOnly, TContext>((val, ctx) =>
            val.Hour >= 12 && val.Hour < 18
                ? null
                : new ValidationError(ctx.Path, "afternoon", message ?? "Must be in the afternoon (12:00 - 18:00)")));

    public TimeOnlyContextSchema<TContext> Evening(string? message = null)
        => Append(new RefinementRule<TimeOnly, TContext>((val, ctx) =>
            val.Hour >= 18
                ? null
                : new ValidationError(ctx.Path, "evening", message ?? "Must be in the evening (after 18:00)")));
}
#endif
