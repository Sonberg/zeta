#if !NETSTANDARD2_0
using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating TimeOnly values.
/// </summary>
public sealed class TimeOnlyContextlessSchema : ContextlessSchema<TimeOnly>
{
    public TimeOnlyContextlessSchema() { }

    public TimeOnlyContextlessSchema Min(TimeOnly min, string? message = null)
    {
        Use(new RefinementRule<TimeOnly>((val, exec) =>
            val >= min
                ? null
                : new ValidationError(exec.Path, "min_time", message ?? $"Must be at or after {min:t}")));
        return this;
    }

    public TimeOnlyContextlessSchema Max(TimeOnly max, string? message = null)
    {
        Use(new RefinementRule<TimeOnly>((val, exec) =>
            val <= max
                ? null
                : new ValidationError(exec.Path, "max_time", message ?? $"Must be at or before {max:t}")));
        return this;
    }

    public TimeOnlyContextlessSchema Between(TimeOnly min, TimeOnly max, string? message = null)
    {
        Use(new RefinementRule<TimeOnly>((val, exec) =>
            val >= min && val <= max
                ? null
                : new ValidationError(exec.Path, "between", message ?? $"Must be between {min:t} and {max:t}")));
        return this;
    }

    public TimeOnlyContextlessSchema BusinessHours(TimeOnly? start = null, TimeOnly? end = null, string? message = null)
    {
        var businessStart = start ?? new TimeOnly(9, 0);
        var businessEnd = end ?? new TimeOnly(17, 0);
        Use(new RefinementRule<TimeOnly>((val, exec) =>
            val >= businessStart && val <= businessEnd
                ? null
                : new ValidationError(exec.Path, "business_hours", message ?? $"Must be during business hours ({businessStart:t} - {businessEnd:t})")));
        return this;
    }

    public TimeOnlyContextlessSchema Morning(string? message = null)
    {
        Use(new RefinementRule<TimeOnly>((val, exec) =>
            val.Hour < 12
                ? null
                : new ValidationError(exec.Path, "morning", message ?? "Must be in the morning (before 12:00)")));
        return this;
    }

    public TimeOnlyContextlessSchema Afternoon(string? message = null)
    {
        Use(new RefinementRule<TimeOnly>((val, exec) =>
            val.Hour >= 12 && val.Hour < 18
                ? null
                : new ValidationError(exec.Path, "afternoon", message ?? "Must be in the afternoon (12:00 - 18:00)")));
        return this;
    }

    public TimeOnlyContextlessSchema Evening(string? message = null)
    {
        Use(new RefinementRule<TimeOnly>((val, exec) =>
            val.Hour >= 18
                ? null
                : new ValidationError(exec.Path, "evening", message ?? "Must be in the evening (after 18:00)")));
        return this;
    }

    public TimeOnlyContextlessSchema Refine(Func<TimeOnly, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<TimeOnly>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    /// <summary>
    /// Creates a context-aware TimeOnly schema with all rules from this schema.
    /// </summary>
    public TimeOnlyContextSchema<TContext> WithContext<TContext>()
        => new TimeOnlyContextSchema<TContext>(Rules.ToContext<TContext>());
}
#endif
