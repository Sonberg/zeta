using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating decimal values with a specific context.
/// </summary>
public class DecimalSchema<TContext> : ISchema<decimal, TContext>
{
    private readonly List<IRule<decimal, TContext>> _rules = [];

    public async Task<Result<decimal>> ValidateAsync(decimal value, ValidationContext<TContext> context)
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
            ? Result<decimal>.Success(value)
            : Result<decimal>.Failure(errors);
    }

    public DecimalSchema<TContext> Use(IRule<decimal, TContext> rule)
    {
        _rules.Add(rule);
        return this;
    }

    public DecimalSchema<TContext> Min(decimal min, string? message = null)
    {
        return Use(new DelegateRule<decimal, TContext>((val, ctx) =>
        {
            if (val >= min) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "min_value", message ?? $"Must be at least {min}"));
        }));
    }

    public DecimalSchema<TContext> Max(decimal max, string? message = null)
    {
        return Use(new DelegateRule<decimal, TContext>((val, ctx) =>
        {
            if (val <= max) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "max_value", message ?? $"Must be at most {max}"));
        }));
    }

    public DecimalSchema<TContext> Positive(string? message = null)
    {
        return Use(new DelegateRule<decimal, TContext>((val, ctx) =>
        {
            if (val > 0) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "positive", message ?? "Must be positive"));
        }));
    }

    public DecimalSchema<TContext> Negative(string? message = null)
    {
        return Use(new DelegateRule<decimal, TContext>((val, ctx) =>
        {
            if (val < 0) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "negative", message ?? "Must be negative"));
        }));
    }

    /// <summary>
    /// Validates that the decimal has at most the specified number of decimal places.
    /// </summary>
    public DecimalSchema<TContext> Precision(int maxDecimalPlaces, string? message = null)
    {
        return Use(new DelegateRule<decimal, TContext>((val, ctx) =>
        {
            var decimalPlaces = GetDecimalPlaces(val);
            if (decimalPlaces <= maxDecimalPlaces) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "precision", message ?? $"Must have at most {maxDecimalPlaces} decimal places"));
        }));
    }

    /// <summary>
    /// Validates that the decimal is a multiple of the specified step.
    /// </summary>
    public DecimalSchema<TContext> MultipleOf(decimal step, string? message = null)
    {
        return Use(new DelegateRule<decimal, TContext>((val, ctx) =>
        {
            if (val % step == 0) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "multiple_of", message ?? $"Must be a multiple of {step}"));
        }));
    }

    public DecimalSchema<TContext> Refine(Func<decimal, TContext, bool> predicate, string message, string code = "custom_error")
    {
        return Use(new DelegateRule<decimal, TContext>((val, ctx) =>
        {
            if (predicate(val, ctx.Data)) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(ctx.Execution.Path, code, message));
        }));
    }

    public DecimalSchema<TContext> Refine(Func<decimal, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }

    private static int GetDecimalPlaces(decimal value)
    {
        value = Math.Abs(value);
        value -= Math.Truncate(value);
        var places = 0;
        while (value > 0)
        {
            places++;
            value *= 10;
            value -= Math.Truncate(value);
        }
        return places;
    }
}

/// <summary>
/// A schema for validating decimal values with default context.
/// </summary>
public sealed class DecimalSchema : DecimalSchema<object?>, ISchema<decimal>
{
    public Task<Result<decimal>> ValidateAsync(decimal value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        return ValidateAsync(value, context);
    }
}
