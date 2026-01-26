#if !NETSTANDARD2_0
using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating TimeOnly values.
/// </summary>
public sealed class TimeOnlySchema : ISchema<TimeOnly>
{
    private readonly RuleEngine<TimeOnly> _rules = new();

    public async ValueTask<Result<TimeOnly>> ValidateAsync(TimeOnly value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var errors = await _rules.ExecuteAsync(value, execution);

        return errors == null
            ? Result<TimeOnly>.Success(value)
            : Result<TimeOnly>.Failure(errors);
    }

    public TimeOnlySchema Min(TimeOnly min, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<TimeOnly>((val, exec) =>
            val >= min
                ? null
                : new ValidationError(exec.Path, "min_time", message ?? $"Must be at or after {min:t}")));
        return this;
    }

    public TimeOnlySchema Max(TimeOnly max, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<TimeOnly>((val, exec) =>
            val <= max
                ? null
                : new ValidationError(exec.Path, "max_time", message ?? $"Must be at or before {max:t}")));
        return this;
    }

    public TimeOnlySchema Between(TimeOnly min, TimeOnly max, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<TimeOnly>((val, exec) =>
            val >= min && val <= max
                ? null
                : new ValidationError(exec.Path, "between", message ?? $"Must be between {min:t} and {max:t}")));
        return this;
    }

    public TimeOnlySchema BusinessHours(TimeOnly? start = null, TimeOnly? end = null, string? message = null)
    {
        var businessStart = start ?? new TimeOnly(9, 0);
        var businessEnd = end ?? new TimeOnly(17, 0);
        _rules.Add(new DelegateValidationRule<TimeOnly>((val, exec) =>
            val >= businessStart && val <= businessEnd
                ? null
                : new ValidationError(exec.Path, "business_hours", message ?? $"Must be during business hours ({businessStart:t} - {businessEnd:t})")));
        return this;
    }

    public TimeOnlySchema Morning(string? message = null)
    {
        _rules.Add(new DelegateValidationRule<TimeOnly>((val, exec) =>
            val.Hour < 12
                ? null
                : new ValidationError(exec.Path, "morning", message ?? "Must be in the morning (before 12:00)")));
        return this;
    }

    public TimeOnlySchema Afternoon(string? message = null)
    {
        _rules.Add(new DelegateValidationRule<TimeOnly>((val, exec) =>
            val.Hour >= 12 && val.Hour < 18
                ? null
                : new ValidationError(exec.Path, "afternoon", message ?? "Must be in the afternoon (12:00 - 18:00)")));
        return this;
    }

    public TimeOnlySchema Evening(string? message = null)
    {
        _rules.Add(new DelegateValidationRule<TimeOnly>((val, exec) =>
            val.Hour >= 18
                ? null
                : new ValidationError(exec.Path, "evening", message ?? "Must be in the evening (after 18:00)")));
        return this;
    }

    public TimeOnlySchema Refine(Func<TimeOnly, bool> predicate, string message, string code = "custom_error")
    {
        _rules.Add(new DelegateValidationRule<TimeOnly>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }
}

/// <summary>
/// A context-aware schema for validating TimeOnly values.
/// </summary>
public class TimeOnlySchema<TContext> : BaseSchema<TimeOnly, TContext>
{
    public TimeOnlySchema<TContext> Min(TimeOnly min, string? message = null)
    {
        Use(new DelegateSyncRule<TimeOnly, TContext>((val, ctx) =>
            val >= min
                ? null
                : new ValidationError(ctx.Execution.Path, "min_time", message ?? $"Must be at or after {min:t}")));
        return this;
    }

    public TimeOnlySchema<TContext> Max(TimeOnly max, string? message = null)
    {
        Use(new DelegateSyncRule<TimeOnly, TContext>((val, ctx) =>
            val <= max
                ? null
                : new ValidationError(ctx.Execution.Path, "max_time", message ?? $"Must be at or before {max:t}")));
        return this;
    }

    public TimeOnlySchema<TContext> Between(TimeOnly min, TimeOnly max, string? message = null)
    {
        Use(new DelegateSyncRule<TimeOnly, TContext>((val, ctx) =>
            val >= min && val <= max
                ? null
                : new ValidationError(ctx.Execution.Path, "between", message ?? $"Must be between {min:t} and {max:t}")));
        return this;
    }

    public TimeOnlySchema<TContext> BusinessHours(TimeOnly? start = null, TimeOnly? end = null, string? message = null)
    {
        var businessStart = start ?? new TimeOnly(9, 0);
        var businessEnd = end ?? new TimeOnly(17, 0);
        Use(new DelegateSyncRule<TimeOnly, TContext>((val, ctx) =>
            val >= businessStart && val <= businessEnd
                ? null
                : new ValidationError(ctx.Execution.Path, "business_hours", message ?? $"Must be during business hours ({businessStart:t} - {businessEnd:t})")));
        return this;
    }

    public TimeOnlySchema<TContext> Morning(string? message = null)
    {
        Use(new DelegateSyncRule<TimeOnly, TContext>((val, ctx) =>
            val.Hour < 12
                ? null
                : new ValidationError(ctx.Execution.Path, "morning", message ?? "Must be in the morning (before 12:00)")));
        return this;
    }

    public TimeOnlySchema<TContext> Afternoon(string? message = null)
    {
        Use(new DelegateSyncRule<TimeOnly, TContext>((val, ctx) =>
            val.Hour >= 12 && val.Hour < 18
                ? null
                : new ValidationError(ctx.Execution.Path, "afternoon", message ?? "Must be in the afternoon (12:00 - 18:00)")));
        return this;
    }

    public TimeOnlySchema<TContext> Evening(string? message = null)
    {
        Use(new DelegateSyncRule<TimeOnly, TContext>((val, ctx) =>
            val.Hour >= 18
                ? null
                : new ValidationError(ctx.Execution.Path, "evening", message ?? "Must be in the evening (after 18:00)")));
        return this;
    }

    public TimeOnlySchema<TContext> Refine(Func<TimeOnly, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new DelegateSyncRule<TimeOnly, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
    }

    public TimeOnlySchema<TContext> Refine(Func<TimeOnly, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}
#endif
