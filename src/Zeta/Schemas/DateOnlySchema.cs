#if !NETSTANDARD2_0
using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating DateOnly values with a specific context.
/// </summary>
public class DateOnlySchema<TContext> : BaseSchema<DateOnly, TContext>
{
    /// <summary>
    /// Validates that the date is at or after the specified minimum.
    /// </summary>
    public DateOnlySchema<TContext> Min(DateOnly min, string? message = null)
    {
        Use(new DelegateSyncRule<DateOnly, TContext>((val, ctx) =>
            val >= min
                ? null
                : new ValidationError(ctx.Execution.Path, "min_date", message ?? $"Must be at or after {min:O}")));
        return this;
    }

    /// <summary>
    /// Validates that the date is at or before the specified maximum.
    /// </summary>
    public DateOnlySchema<TContext> Max(DateOnly max, string? message = null)
    {
        Use(new DelegateSyncRule<DateOnly, TContext>((val, ctx) =>
            val <= max
                ? null
                : new ValidationError(ctx.Execution.Path, "max_date", message ?? $"Must be at or before {max:O}")));
        return this;
    }

    /// <summary>
    /// Validates that the date is in the past (before today).
    /// </summary>
    public DateOnlySchema<TContext> Past(string? message = null)
    {
        Use(new DelegateSyncRule<DateOnly, TContext>((val, ctx) =>
        {
            var today = DateOnly.FromDateTime(ctx.Execution.TimeProvider.GetUtcNow().UtcDateTime);
            return val < today
                ? null
                : new ValidationError(ctx.Execution.Path, "past", message ?? "Must be in the past");
        }));
        return this;
    }

    /// <summary>
    /// Validates that the date is in the future (after today).
    /// </summary>
    public DateOnlySchema<TContext> Future(string? message = null)
    {
        Use(new DelegateSyncRule<DateOnly, TContext>((val, ctx) =>
        {
            var today = DateOnly.FromDateTime(ctx.Execution.TimeProvider.GetUtcNow().UtcDateTime);
            return val > today
                ? null
                : new ValidationError(ctx.Execution.Path, "future", message ?? "Must be in the future");
        }));
        return this;
    }

    /// <summary>
    /// Validates that the date is within a specified range.
    /// </summary>
    public DateOnlySchema<TContext> Between(DateOnly min, DateOnly max, string? message = null)
    {
        Use(new DelegateSyncRule<DateOnly, TContext>((val, ctx) =>
            val >= min && val <= max
                ? null
                : new ValidationError(ctx.Execution.Path, "between", message ?? $"Must be between {min:O} and {max:O}")));
        return this;
    }

    /// <summary>
    /// Validates that the date falls on a weekday (Monday-Friday).
    /// </summary>
    public DateOnlySchema<TContext> Weekday(string? message = null)
    {
        Use(new DelegateSyncRule<DateOnly, TContext>((val, ctx) =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday
                ? null
                : new ValidationError(ctx.Execution.Path, "weekday", message ?? "Must be a weekday")));
        return this;
    }

    /// <summary>
    /// Validates that the date falls on a weekend (Saturday or Sunday).
    /// </summary>
    public DateOnlySchema<TContext> Weekend(string? message = null)
    {
        Use(new DelegateSyncRule<DateOnly, TContext>((val, ctx) =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday
                ? null
                : new ValidationError(ctx.Execution.Path, "weekend", message ?? "Must be a weekend")));
        return this;
    }

    /// <summary>
    /// Validates that the user is at least the specified age based on the date (birthdate validation).
    /// </summary>
    public DateOnlySchema<TContext> MinAge(int years, string? message = null)
    {
        Use(new DelegateSyncRule<DateOnly, TContext>((val, ctx) =>
        {
            var today = DateOnly.FromDateTime(ctx.Execution.TimeProvider.GetUtcNow().UtcDateTime);
            var age = today.Year - val.Year;
            if (val > today.AddYears(-age)) age--;

            return age >= years
                ? null
                : new ValidationError(ctx.Execution.Path, "min_age", message ?? $"Must be at least {years} years old");
        }));
        return this;
    }

    /// <summary>
    /// Validates that the user is at most the specified age based on the date (birthdate validation).
    /// </summary>
    public DateOnlySchema<TContext> MaxAge(int years, string? message = null)
    {
        Use(new DelegateSyncRule<DateOnly, TContext>((val, ctx) =>
        {
            var today = DateOnly.FromDateTime(ctx.Execution.TimeProvider.GetUtcNow().UtcDateTime);
            var age = today.Year - val.Year;
            if (val > today.AddYears(-age)) age--;

            return age <= years
                ? null
                : new ValidationError(ctx.Execution.Path, "max_age", message ?? $"Must be at most {years} years old");
        }));
        return this;
    }

    public DateOnlySchema<TContext> Refine(Func<DateOnly, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new DelegateSyncRule<DateOnly, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
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
    public async ValueTask<Result<DateOnly>> ValidateAsync(DateOnly value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        var result = await ValidateAsync(value, context);

        return result.IsSuccess
            ? Result<DateOnly>.Success(value)
            : Result<DateOnly>.Failure(result.Errors);
    }
}
#endif
