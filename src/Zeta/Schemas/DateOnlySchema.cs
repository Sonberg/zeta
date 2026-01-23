using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating DateOnly values with a specific context.
/// </summary>
public class DateOnlySchema<TContext> : ISchema<DateOnly, TContext>
{
    private readonly List<IRule<DateOnly, TContext>> _rules = [];

    public async ValueTask<Result<DateOnly>> ValidateAsync(DateOnly value, ValidationContext<TContext> context)
    {
        List<ValidationError>? errors = null;
        foreach (var rule in _rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error != null)
            {
                errors ??= new List<ValidationError>();
                errors.Add(error);
            }
        }

        return errors == null
            ? Result<DateOnly>.Success(value)
            : Result<DateOnly>.Failure(errors);
    }

    public DateOnlySchema<TContext> Use(IRule<DateOnly, TContext> rule)
    {
        _rules.Add(rule);
        return this;
    }

    /// <summary>
    /// Validates that the date is at or after the specified minimum.
    /// </summary>
    public DateOnlySchema<TContext> Min(DateOnly min, string? message = null)
    {
        return Use(new DelegateRule<DateOnly, TContext>((val, ctx) =>
        {
            if (val >= min) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "min_date", message ?? $"Must be at or after {min:O}"));
        }));
    }

    /// <summary>
    /// Validates that the date is at or before the specified maximum.
    /// </summary>
    public DateOnlySchema<TContext> Max(DateOnly max, string? message = null)
    {
        return Use(new DelegateRule<DateOnly, TContext>((val, ctx) =>
        {
            if (val <= max) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "max_date", message ?? $"Must be at or before {max:O}"));
        }));
    }

    /// <summary>
    /// Validates that the date is in the past (before today).
    /// </summary>
    public DateOnlySchema<TContext> Past(string? message = null)
    {
        return Use(new DelegateRule<DateOnly, TContext>((val, ctx) =>
        {
            if (val < DateOnly.FromDateTime(DateTime.UtcNow)) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "past", message ?? "Must be in the past"));
        }));
    }

    /// <summary>
    /// Validates that the date is in the future (after today).
    /// </summary>
    public DateOnlySchema<TContext> Future(string? message = null)
    {
        return Use(new DelegateRule<DateOnly, TContext>((val, ctx) =>
        {
            if (val > DateOnly.FromDateTime(DateTime.UtcNow)) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "future", message ?? "Must be in the future"));
        }));
    }

    /// <summary>
    /// Validates that the date is within a specified range.
    /// </summary>
    public DateOnlySchema<TContext> Between(DateOnly min, DateOnly max, string? message = null)
    {
        return Use(new DelegateRule<DateOnly, TContext>((val, ctx) =>
        {
            if (val >= min && val <= max) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "between", message ?? $"Must be between {min:O} and {max:O}"));
        }));
    }

    /// <summary>
    /// Validates that the date falls on a weekday (Monday-Friday).
    /// </summary>
    public DateOnlySchema<TContext> Weekday(string? message = null)
    {
        return Use(new DelegateRule<DateOnly, TContext>((val, ctx) =>
        {
            if (val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday)
                return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "weekday", message ?? "Must be a weekday"));
        }));
    }

    /// <summary>
    /// Validates that the date falls on a weekend (Saturday or Sunday).
    /// </summary>
    public DateOnlySchema<TContext> Weekend(string? message = null)
    {
        return Use(new DelegateRule<DateOnly, TContext>((val, ctx) =>
        {
            if (val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday)
                return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "weekend", message ?? "Must be a weekend"));
        }));
    }

    /// <summary>
    /// Validates that the user is at least the specified age based on the date (birthdate validation).
    /// </summary>
    public DateOnlySchema<TContext> MinAge(int years, string? message = null)
    {
        return Use(new DelegateRule<DateOnly, TContext>((val, ctx) =>
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - val.Year;
            if (val > today.AddYears(-age)) age--;

            if (age >= years) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "min_age", message ?? $"Must be at least {years} years old"));
        }));
    }

    /// <summary>
    /// Validates that the user is at most the specified age based on the date (birthdate validation).
    /// </summary>
    public DateOnlySchema<TContext> MaxAge(int years, string? message = null)
    {
        return Use(new DelegateRule<DateOnly, TContext>((val, ctx) =>
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - val.Year;
            if (val > today.AddYears(-age)) age--;

            if (age <= years) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "max_age", message ?? $"Must be at most {years} years old"));
        }));
    }

    public DateOnlySchema<TContext> Refine(Func<DateOnly, TContext, bool> predicate, string message, string code = "custom_error")
    {
        return Use(new DelegateRule<DateOnly, TContext>((val, ctx) =>
        {
            if (predicate(val, ctx.Data)) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(ctx.Execution.Path, code, message));
        }));
    }

    public DateOnlySchema<TContext> Refine(Func<DateOnly, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}

/// <summary>
/// A schema for validating DateOnly values with default context.
/// </summary>
public sealed class DateOnlySchema : DateOnlySchema<object?>, ISchema<DateOnly>
{
    public ValueTask<Result<DateOnly>> ValidateAsync(DateOnly value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        return ValidateAsync(value, context);
    }
}
