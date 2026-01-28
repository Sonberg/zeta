#if !NETSTANDARD2_0
using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating DateOnly values.
/// </summary>
public sealed class DateOnlyContextlessSchema : ContextlessSchema<DateOnly>
{
    public DateOnlyContextlessSchema() { }

    public DateOnlyContextlessSchema Min(DateOnly min, string? message = null)
    {
        Use(new RefinementRule<DateOnly>((val, exec) =>
            val >= min
                ? null
                : new ValidationError(exec.Path, "min_date", message ?? $"Must be at or after {min:O}")));
        return this;
    }

    public DateOnlyContextlessSchema Max(DateOnly max, string? message = null)
    {
        Use(new RefinementRule<DateOnly>((val, exec) =>
            val <= max
                ? null
                : new ValidationError(exec.Path, "max_date", message ?? $"Must be at or before {max:O}")));
        return this;
    }

    public DateOnlyContextlessSchema Past(string? message = null)
    {
        Use(new RefinementRule<DateOnly>((val, exec) =>
        {
            var today = DateOnly.FromDateTime(exec.TimeProvider.GetUtcNow().UtcDateTime);
            return val < today
                ? null
                : new ValidationError(exec.Path, "past", message ?? "Must be in the past");
        }));
        return this;
    }

    public DateOnlyContextlessSchema Future(string? message = null)
    {
        Use(new RefinementRule<DateOnly>((val, exec) =>
        {
            var today = DateOnly.FromDateTime(exec.TimeProvider.GetUtcNow().UtcDateTime);
            return val > today
                ? null
                : new ValidationError(exec.Path, "future", message ?? "Must be in the future");
        }));
        return this;
    }

    public DateOnlyContextlessSchema Between(DateOnly min, DateOnly max, string? message = null)
    {
        Use(new RefinementRule<DateOnly>((val, exec) =>
            val >= min && val <= max
                ? null
                : new ValidationError(exec.Path, "between", message ?? $"Must be between {min:O} and {max:O}")));
        return this;
    }

    public DateOnlyContextlessSchema Weekday(string? message = null)
    {
        Use(new RefinementRule<DateOnly>((val, exec) =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday
                ? null
                : new ValidationError(exec.Path, "weekday", message ?? "Must be a weekday")));
        return this;
    }

    public DateOnlyContextlessSchema Weekend(string? message = null)
    {
        Use(new RefinementRule<DateOnly>((val, exec) =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday
                ? null
                : new ValidationError(exec.Path, "weekend", message ?? "Must be a weekend")));
        return this;
    }

    public DateOnlyContextlessSchema MinAge(int years, string? message = null)
    {
        Use(new RefinementRule<DateOnly>((val, exec) =>
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

    public DateOnlyContextlessSchema MaxAge(int years, string? message = null)
    {
        Use(new RefinementRule<DateOnly>((val, exec) =>
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

    public DateOnlyContextlessSchema Refine(Func<DateOnly, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<DateOnly>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    public DateOnlyContextlessSchema RefineAsync(
        Func<DateOnly, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        Use(new RefinementRule<DateOnly>(async (val, exec) =>
            await predicate(val, exec.CancellationToken)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    /// <summary>
    /// Creates a context-aware DateOnly schema with all rules from this schema.
    /// </summary>
    public DateOnlyContextSchema<TContext> WithContext<TContext>()
        => new DateOnlyContextSchema<TContext>(Rules.ToContext<TContext>());
}
#endif
