using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating double values with a specific context.
/// </summary>
public class DoubleSchema<TContext> : BaseSchema<double, TContext>
{
    public DoubleSchema<TContext> Min(double min, string? message = null)
    {
        Use(new DelegateSyncRule<double, TContext>((val, ctx) =>
            val >= min
                ? null
                : new ValidationError(ctx.Execution.Path, "min_value", message ?? $"Must be at least {min}")));
        return this;
    }

    public DoubleSchema<TContext> Max(double max, string? message = null)
    {
        Use(new DelegateSyncRule<double, TContext>((val, ctx) =>
            val <= max
                ? null
                : new ValidationError(ctx.Execution.Path, "max_value", message ?? $"Must be at most {max}")));
        return this;
    }

    public DoubleSchema<TContext> Positive(string? message = null)
    {
        Use(new DelegateSyncRule<double, TContext>((val, ctx) =>
            val > 0
                ? null
                : new ValidationError(ctx.Execution.Path, "positive", message ?? "Must be positive")));
        return this;
    }

    public DoubleSchema<TContext> Negative(string? message = null)
    {
        Use(new DelegateSyncRule<double, TContext>((val, ctx) =>
            val < 0
                ? null
                : new ValidationError(ctx.Execution.Path, "negative", message ?? "Must be negative")));
        return this;
    }

    public DoubleSchema<TContext> Finite(string? message = null)
    {
        Use(new DelegateSyncRule<double, TContext>((val, ctx) =>
            !double.IsNaN(val) && !double.IsInfinity(val)
                ? null
                : new ValidationError(ctx.Execution.Path, "finite", message ?? "Must be a finite number")));
        return this;
    }

    public DoubleSchema<TContext> Refine(Func<double, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new DelegateSyncRule<double, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
    }

    public DoubleSchema<TContext> Refine(Func<double, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}

/// <summary>
/// A schema for validating double values with default context.
/// </summary>
public sealed class DoubleSchema : DoubleSchema<object?>, ISchema<double>
{
    public async ValueTask<Result<double>> ValidateAsync(double value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        var result = await ValidateAsync(value, context);

        return result.IsSuccess
            ? Result<double>.Success(value)
            : Result<double>.Failure(result.Errors);
    }
}
