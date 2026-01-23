using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating DateTime values with a specific context.
/// </summary>
public class DateTimeSchema<TContext> : ISchema<DateTime, TContext>
{
    private readonly List<IRule<DateTime, TContext>> _rules = [];

    public async ValueTask<Result<DateTime>> ValidateAsync(DateTime value, ValidationContext<TContext> context)
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
            ? Result<DateTime>.Success(value)
            : Result<DateTime>.Failure(errors);
    }

    public DateTimeSchema<TContext> Use(IRule<DateTime, TContext> rule)
    {
        _rules.Add(rule);
        return this;
    }

    /// <summary>
    /// Validates that the date is at or after the specified minimum.
    /// </summary>
    public DateTimeSchema<TContext> Min(DateTime min, string? message = null)
    {
        return Use(new DelegateRule<DateTime, TContext>((val, ctx) =>
        {
            if (val >= min) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "min_date", message ?? $"Must be at or after {min:O}"));
        }));
    }

    /// <summary>
    /// Validates that the date is at or before the specified maximum.
    /// </summary>
    public DateTimeSchema<TContext> Max(DateTime max, string? message = null)
    {
        return Use(new DelegateRule<DateTime, TContext>((val, ctx) =>
        {
            if (val <= max) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "max_date", message ?? $"Must be at or before {max:O}"));
        }));
    }

    /// <summary>
    /// Validates that the date is in the past (before DateTime.UtcNow).
    /// </summary>
    public DateTimeSchema<TContext> Past(string? message = null)
    {
        return Use(new DelegateRule<DateTime, TContext>((val, ctx) =>
        {
            if (val < DateTime.UtcNow) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "past", message ?? "Must be in the past"));
        }));
    }

    /// <summary>
    /// Validates that the date is in the future (after DateTime.UtcNow).
    /// </summary>
    public DateTimeSchema<TContext> Future(string? message = null)
    {
        return Use(new DelegateRule<DateTime, TContext>((val, ctx) =>
        {
            if (val > DateTime.UtcNow) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "future", message ?? "Must be in the future"));
        }));
    }

    /// <summary>
    /// Validates that the date is within a specified range.
    /// </summary>
    public DateTimeSchema<TContext> Between(DateTime min, DateTime max, string? message = null)
    {
        return Use(new DelegateRule<DateTime, TContext>((val, ctx) =>
        {
            if (val >= min && val <= max) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "between", message ?? $"Must be between {min:O} and {max:O}"));
        }));
    }

    /// <summary>
    /// Validates that the date falls on a weekday (Monday-Friday).
    /// </summary>
    public DateTimeSchema<TContext> Weekday(string? message = null)
    {
        return Use(new DelegateRule<DateTime, TContext>((val, ctx) =>
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
    public DateTimeSchema<TContext> Weekend(string? message = null)
    {
        return Use(new DelegateRule<DateTime, TContext>((val, ctx) =>
        {
            if (val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday)
                return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "weekend", message ?? "Must be a weekend"));
        }));
    }

    /// <summary>
    /// Validates that the date is within the specified number of days from now.
    /// </summary>
    public DateTimeSchema<TContext> WithinDays(int days, string? message = null)
    {
        return Use(new DelegateRule<DateTime, TContext>((val, ctx) =>
        {
            var diff = Math.Abs((val - DateTime.UtcNow).TotalDays);
            if (diff <= days) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "within_days", message ?? $"Must be within {days} days from now"));
        }));
    }

    /// <summary>
    /// Validates that the user is at least the specified age based on the date (birthdate validation).
    /// </summary>
    public DateTimeSchema<TContext> MinAge(int years, string? message = null)
    {
        return Use(new DelegateRule<DateTime, TContext>((val, ctx) =>
        {
            var today = DateTime.UtcNow.Date;
            var age = today.Year - val.Year;
            if (val.Date > today.AddYears(-age)) age--;

            if (age >= years) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "min_age", message ?? $"Must be at least {years} years old"));
        }));
    }

    /// <summary>
    /// Validates that the user is at most the specified age based on the date (birthdate validation).
    /// </summary>
    public DateTimeSchema<TContext> MaxAge(int years, string? message = null)
    {
        return Use(new DelegateRule<DateTime, TContext>((val, ctx) =>
        {
            var today = DateTime.UtcNow.Date;
            var age = today.Year - val.Year;
            if (val.Date > today.AddYears(-age)) age--;

            if (age <= years) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "max_age", message ?? $"Must be at most {years} years old"));
        }));
    }

    public DateTimeSchema<TContext> Refine(Func<DateTime, TContext, bool> predicate, string message, string code = "custom_error")
    {
        return Use(new DelegateRule<DateTime, TContext>((val, ctx) =>
        {
            if (predicate(val, ctx.Data)) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(ctx.Execution.Path, code, message));
        }));
    }

    public DateTimeSchema<TContext> Refine(Func<DateTime, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}

/// <summary>
/// A schema for validating DateTime values with default context.
/// </summary>
public sealed class DateTimeSchema : DateTimeSchema<object?>, ISchema<DateTime>
{
    public ValueTask<Result<DateTime>> ValidateAsync(DateTime value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        return ValidateAsync(value, context);
    }
}
