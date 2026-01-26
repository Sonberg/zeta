#if !NETSTANDARD2_0
using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating DateOnly values.
/// </summary>
public sealed class DateOnlySchema : ISchema<DateOnly>
{
    private readonly RuleEngine<DateOnly> _rules = new();

    public async ValueTask<Result<DateOnly>> ValidateAsync(DateOnly value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var errors = await _rules.ExecuteAsync(value, execution);

        return errors == null
            ? Result<DateOnly>.Success(value)
            : Result<DateOnly>.Failure(errors);
    }

    public DateOnlySchema Min(DateOnly min, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<DateOnly>((val, exec) =>
            val >= min
                ? null
                : new ValidationError(exec.Path, "min_date", message ?? $"Must be at or after {min:O}")));
        return this;
    }

    public DateOnlySchema Max(DateOnly max, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<DateOnly>((val, exec) =>
            val <= max
                ? null
                : new ValidationError(exec.Path, "max_date", message ?? $"Must be at or before {max:O}")));
        return this;
    }

    public DateOnlySchema Past(string? message = null)
    {
        _rules.Add(new DelegateValidationRule<DateOnly>((val, exec) =>
        {
            var today = DateOnly.FromDateTime(exec.TimeProvider.GetUtcNow().UtcDateTime);
            return val < today
                ? null
                : new ValidationError(exec.Path, "past", message ?? "Must be in the past");
        }));
        return this;
    }

    public DateOnlySchema Future(string? message = null)
    {
        _rules.Add(new DelegateValidationRule<DateOnly>((val, exec) =>
        {
            var today = DateOnly.FromDateTime(exec.TimeProvider.GetUtcNow().UtcDateTime);
            return val > today
                ? null
                : new ValidationError(exec.Path, "future", message ?? "Must be in the future");
        }));
        return this;
    }

    public DateOnlySchema Between(DateOnly min, DateOnly max, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<DateOnly>((val, exec) =>
            val >= min && val <= max
                ? null
                : new ValidationError(exec.Path, "between", message ?? $"Must be between {min:O} and {max:O}")));
        return this;
    }

    public DateOnlySchema Weekday(string? message = null)
    {
        _rules.Add(new DelegateValidationRule<DateOnly>((val, exec) =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday
                ? null
                : new ValidationError(exec.Path, "weekday", message ?? "Must be a weekday")));
        return this;
    }

    public DateOnlySchema Weekend(string? message = null)
    {
        _rules.Add(new DelegateValidationRule<DateOnly>((val, exec) =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday
                ? null
                : new ValidationError(exec.Path, "weekend", message ?? "Must be a weekend")));
        return this;
    }

    public DateOnlySchema MinAge(int years, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<DateOnly>((val, exec) =>
        {
            var today = DateOnly.FromDateTime(exec.TimeProvider.GetUtcNow().UtcDateTime);
            var age = today.Year - val.Year;
            if (val > today.AddYears(-age)) age--;

            return age >= years
                ? null
                : new ValidationError(exec.Path, "min_age", message ?? $"Must be at least {years} years old");
        }));
        return this;
    }

    public DateOnlySchema MaxAge(int years, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<DateOnly>((val, exec) =>
        {
            var today = DateOnly.FromDateTime(exec.TimeProvider.GetUtcNow().UtcDateTime);
            var age = today.Year - val.Year;
            if (val > today.AddYears(-age)) age--;

            return age <= years
                ? null
                : new ValidationError(exec.Path, "max_age", message ?? $"Must be at most {years} years old");
        }));
        return this;
    }

    public DateOnlySchema Refine(Func<DateOnly, bool> predicate, string message, string code = "custom_error")
    {
        _rules.Add(new DelegateValidationRule<DateOnly>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }
}

/// <summary>
/// A context-aware schema for validating DateOnly values.
/// </summary>
public class DateOnlySchema<TContext> : BaseSchema<DateOnly, TContext>
{
    public DateOnlySchema<TContext> Min(DateOnly min, string? message = null)
    {
        Use(new DelegateSyncRule<DateOnly, TContext>((val, ctx) =>
            val >= min
                ? null
                : new ValidationError(ctx.Execution.Path, "min_date", message ?? $"Must be at or after {min:O}")));
        return this;
    }

    public DateOnlySchema<TContext> Max(DateOnly max, string? message = null)
    {
        Use(new DelegateSyncRule<DateOnly, TContext>((val, ctx) =>
            val <= max
                ? null
                : new ValidationError(ctx.Execution.Path, "max_date", message ?? $"Must be at or before {max:O}")));
        return this;
    }

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

    public DateOnlySchema<TContext> Between(DateOnly min, DateOnly max, string? message = null)
    {
        Use(new DelegateSyncRule<DateOnly, TContext>((val, ctx) =>
            val >= min && val <= max
                ? null
                : new ValidationError(ctx.Execution.Path, "between", message ?? $"Must be between {min:O} and {max:O}")));
        return this;
    }

    public DateOnlySchema<TContext> Weekday(string? message = null)
    {
        Use(new DelegateSyncRule<DateOnly, TContext>((val, ctx) =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday
                ? null
                : new ValidationError(ctx.Execution.Path, "weekday", message ?? "Must be a weekday")));
        return this;
    }

    public DateOnlySchema<TContext> Weekend(string? message = null)
    {
        Use(new DelegateSyncRule<DateOnly, TContext>((val, ctx) =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday
                ? null
                : new ValidationError(ctx.Execution.Path, "weekend", message ?? "Must be a weekend")));
        return this;
    }

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
#endif
