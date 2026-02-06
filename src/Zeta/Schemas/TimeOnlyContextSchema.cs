#if !NETSTANDARD2_0
using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating TimeOnly values.
/// </summary>
public class TimeOnlyContextSchema<TContext> : ContextSchema<TimeOnly?, TContext>
{
    public TimeOnlyContextSchema() { }

    public TimeOnlyContextSchema(ContextRuleEngine<TimeOnly?, TContext> rules) : base(rules) { }

    public TimeOnlyContextSchema<TContext> Min(TimeOnly min, string? message = null)
    {
        Use(new RefinementRule<TimeOnly?, TContext>((val, ctx) =>
            val >= min
                ? null
                : new ValidationError(ctx.Path, "min_time", message ?? $"Must be at or after {min:t}")));
        return this;
    }

    public TimeOnlyContextSchema<TContext> Max(TimeOnly max, string? message = null)
    {
        Use(new RefinementRule<TimeOnly?, TContext>((val, ctx) =>
            val <= max
                ? null
                : new ValidationError(ctx.Path, "max_time", message ?? $"Must be at or before {max:t}")));
        return this;
    }

    public TimeOnlyContextSchema<TContext> Between(TimeOnly min, TimeOnly max, string? message = null)
    {
        Use(new RefinementRule<TimeOnly?, TContext>((val, ctx) =>
            val >= min && val <= max
                ? null
                : new ValidationError(ctx.Path, "between", message ?? $"Must be between {min:t} and {max:t}")));
        return this;
    }

    public TimeOnlyContextSchema<TContext> BusinessHours(TimeOnly? start = null, TimeOnly? end = null, string? message = null)
    {
        var businessStart = start ?? new TimeOnly(9, 0);
        var businessEnd = end ?? new TimeOnly(17, 0);
        Use(new RefinementRule<TimeOnly?, TContext>((val, ctx) =>
            val >= businessStart && val <= businessEnd
                ? null
                : new ValidationError(ctx.Path, "business_hours", message ?? $"Must be during business hours ({businessStart:t} - {businessEnd:t})")));
        return this;
    }

    public TimeOnlyContextSchema<TContext> Morning(string? message = null)
    {
        Use(new RefinementRule<TimeOnly?, TContext>((val, ctx) =>
            val.Hour < 12
                ? null
                : new ValidationError(ctx.Path, "morning", message ?? "Must be in the morning (before 12:00)")));
        return this;
    }

    public TimeOnlyContextSchema<TContext> Afternoon(string? message = null)
    {
        Use(new RefinementRule<TimeOnly?, TContext>((val, ctx) =>
            val.Hour >= 12 && val.Hour < 18
                ? null
                : new ValidationError(ctx.Path, "afternoon", message ?? "Must be in the afternoon (12:00 - 18:00)")));
        return this;
    }

    public TimeOnlyContextSchema<TContext> Evening(string? message = null)
    {
        Use(new RefinementRule<TimeOnly?, TContext>((val, ctx) =>
            val.Hour >= 18
                ? null
                : new ValidationError(ctx.Path, "evening", message ?? "Must be in the evening (after 18:00)")));
        return this;
    }

    public TimeOnlyContextSchema<TContext> Refine(Func<TimeOnly?, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<TimeOnly?, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public TimeOnlyContextSchema<TContext> Refine(Func<TimeOnly?, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }

    public TimeOnlyContextSchema<TContext> RefineAsync(
        Func<TimeOnly?, TContext, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        Use(new RefinementRule<TimeOnly?, TContext>(async (val, ctx) =>
            await predicate(val, ctx.Data, ctx.CancellationToken)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public TimeOnlyContextSchema<TContext> RefineAsync(
        Func<TimeOnly?, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        return RefineAsync((val, _, ct) => predicate(val, ct), message, code);
    }
}
#endif
