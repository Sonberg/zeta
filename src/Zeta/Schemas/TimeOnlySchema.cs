#if !NETSTANDARD2_0
using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating TimeOnly values with a specific context.
/// </summary>
public class TimeOnlySchema<TContext> : ISchema<TimeOnly, TContext>
{
    private readonly List<IRule<TimeOnly, TContext>> _rules = [];

    public async ValueTask<Result> ValidateAsync(TimeOnly value, ValidationContext<TContext> context)
    {
        List<ValidationError>? errors = null;
        foreach (var rule in _rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error == null) continue;
            errors ??= [];
            errors.Add(error);
        }

        return errors == null
            ? Result.Success()
            : Result.Failure(errors);
    }

    public TimeOnlySchema<TContext> Use(IRule<TimeOnly, TContext> rule)
    {
        _rules.Add(rule);
        return this;
    }

    /// <summary>
    /// Validates that the time is at or after the specified minimum.
    /// </summary>
    public TimeOnlySchema<TContext> Min(TimeOnly min, string? message = null)
    {
        return Use(new DelegateRule<TimeOnly, TContext>((val, ctx) =>
        {
            if (val >= min) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "min_time", message ?? $"Must be at or after {min:t}"));
        }));
    }

    /// <summary>
    /// Validates that the time is at or before the specified maximum.
    /// </summary>
    public TimeOnlySchema<TContext> Max(TimeOnly max, string? message = null)
    {
        return Use(new DelegateRule<TimeOnly, TContext>((val, ctx) =>
        {
            if (val <= max) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "max_time", message ?? $"Must be at or before {max:t}"));
        }));
    }

    /// <summary>
    /// Validates that the time is within a specified range.
    /// </summary>
    public TimeOnlySchema<TContext> Between(TimeOnly min, TimeOnly max, string? message = null)
    {
        return Use(new DelegateRule<TimeOnly, TContext>((val, ctx) =>
        {
            if (val >= min && val <= max) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "between", message ?? $"Must be between {min:t} and {max:t}"));
        }));
    }

    /// <summary>
    /// Validates that the time is during business hours (9 AM - 5 PM by default).
    /// </summary>
    public TimeOnlySchema<TContext> BusinessHours(TimeOnly? start = null, TimeOnly? end = null, string? message = null)
    {
        var businessStart = start ?? new TimeOnly(9, 0);
        var businessEnd = end ?? new TimeOnly(17, 0);
        return Use(new DelegateRule<TimeOnly, TContext>((val, ctx) =>
        {
            if (val >= businessStart && val <= businessEnd) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "business_hours", message ?? $"Must be during business hours ({businessStart:t} - {businessEnd:t})"));
        }));
    }

    /// <summary>
    /// Validates that the time falls within morning hours (before noon).
    /// </summary>
    public TimeOnlySchema<TContext> Morning(string? message = null)
    {
        return Use(new DelegateRule<TimeOnly, TContext>((val, ctx) =>
        {
            if (val.Hour < 12) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "morning", message ?? "Must be in the morning (before 12:00)"));
        }));
    }

    /// <summary>
    /// Validates that the time falls within afternoon hours (noon to 6 PM).
    /// </summary>
    public TimeOnlySchema<TContext> Afternoon(string? message = null)
    {
        return Use(new DelegateRule<TimeOnly, TContext>((val, ctx) =>
        {
            if (val.Hour >= 12 && val.Hour < 18) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "afternoon", message ?? "Must be in the afternoon (12:00 - 18:00)"));
        }));
    }

    /// <summary>
    /// Validates that the time falls within evening hours (6 PM to midnight).
    /// </summary>
    public TimeOnlySchema<TContext> Evening(string? message = null)
    {
        return Use(new DelegateRule<TimeOnly, TContext>((val, ctx) =>
        {
            if (val.Hour >= 18) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "evening", message ?? "Must be in the evening (after 18:00)"));
        }));
    }

    public TimeOnlySchema<TContext> Refine(Func<TimeOnly, TContext, bool> predicate, string message, string code = "custom_error")
    {
        return Use(new DelegateRule<TimeOnly, TContext>((val, ctx) =>
        {
            if (predicate(val, ctx.Data)) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(ctx.Execution.Path, code, message));
        }));
    }

    public TimeOnlySchema<TContext> Refine(Func<TimeOnly, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}

/// <summary>
/// A schema for validating TimeOnly values with default context.
/// </summary>
public sealed class TimeOnlySchema : TimeOnlySchema<object?>, ISchema<TimeOnly>
{
    public async ValueTask<Result<TimeOnly>> ValidateAsync(TimeOnly value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        var result = await ValidateAsync(value, context);

        return result.IsSuccess
            ? Result<TimeOnly>.Success(value)
            : Result<TimeOnly>.Failure(result.Errors);
        ;
    }
}
#endif