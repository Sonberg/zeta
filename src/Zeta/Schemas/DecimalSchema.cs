using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating decimal values with a specific context.
/// </summary>
public class DecimalSchema<TContext> : BaseSchema<decimal, TContext>
{
    public DecimalSchema<TContext> Min(decimal min, string? message = null)
    {
        Use(new DelegateSyncRule<decimal, TContext>((val, ctx) =>
            val >= min
                ? null
                : new ValidationError(ctx.Execution.Path, "min_value", message ?? $"Must be at least {min}")));
        return this;
    }

    public DecimalSchema<TContext> Max(decimal max, string? message = null)
    {
        Use(new DelegateSyncRule<decimal, TContext>((val, ctx) =>
            val <= max
                ? null
                : new ValidationError(ctx.Execution.Path, "max_value", message ?? $"Must be at most {max}")));
        return this;
    }

    public DecimalSchema<TContext> Positive(string? message = null)
    {
        Use(new DelegateSyncRule<decimal, TContext>((val, ctx) =>
            val > 0
                ? null
                : new ValidationError(ctx.Execution.Path, "positive", message ?? "Must be positive")));
        return this;
    }

    public DecimalSchema<TContext> Negative(string? message = null)
    {
        Use(new DelegateSyncRule<decimal, TContext>((val, ctx) =>
            val < 0
                ? null
                : new ValidationError(ctx.Execution.Path, "negative", message ?? "Must be negative")));
        return this;
    }

    /// <summary>
    /// Validates that the decimal has at most the specified number of decimal places.
    /// </summary>
    public DecimalSchema<TContext> Precision(int maxDecimalPlaces, string? message = null)
    {
        Use(new DelegateSyncRule<decimal, TContext>((val, ctx) =>
            GetDecimalPlaces(val) <= maxDecimalPlaces
                ? null
                : new ValidationError(ctx.Execution.Path, "precision", message ?? $"Must have at most {maxDecimalPlaces} decimal places")));
        return this;
    }

    /// <summary>
    /// Validates that the decimal is a multiple of the specified step.
    /// </summary>
    public DecimalSchema<TContext> MultipleOf(decimal step, string? message = null)
    {
        Use(new DelegateSyncRule<decimal, TContext>((val, ctx) =>
            val % step == 0
                ? null
                : new ValidationError(ctx.Execution.Path, "multiple_of", message ?? $"Must be a multiple of {step}")));
        return this;
    }

    public DecimalSchema<TContext> Refine(Func<decimal, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new DelegateSyncRule<decimal, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
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
    public async ValueTask<Result<decimal>> ValidateAsync(decimal value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        var result = await ValidateAsync(value, context);

        return result.IsSuccess
            ? Result<decimal>.Success(value)
            : Result<decimal>.Failure(result.Errors);
    }
}
