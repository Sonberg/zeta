using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating double values with a specific context.
/// </summary>
public class DoubleSchema<TContext> : ISchema<double, TContext>
{
    private readonly List<IRule<double, TContext>> _rules = [];

    public async Task<Result<double>> ValidateAsync(double value, ValidationContext<TContext> context)
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
            ? Result<double>.Success(value)
            : Result<double>.Failure(errors);
    }

    public DoubleSchema<TContext> Use(IRule<double, TContext> rule)
    {
        _rules.Add(rule);
        return this;
    }

    public DoubleSchema<TContext> Min(double min, string? message = null)
    {
        return Use(new DelegateRule<double, TContext>((val, ctx) =>
        {
            if (val >= min) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "min_value", message ?? $"Must be at least {min}"));
        }));
    }

    public DoubleSchema<TContext> Max(double max, string? message = null)
    {
        return Use(new DelegateRule<double, TContext>((val, ctx) =>
        {
            if (val <= max) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "max_value", message ?? $"Must be at most {max}"));
        }));
    }

    public DoubleSchema<TContext> Positive(string? message = null)
    {
        return Use(new DelegateRule<double, TContext>((val, ctx) =>
        {
            if (val > 0) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "positive", message ?? "Must be positive"));
        }));
    }

    public DoubleSchema<TContext> Negative(string? message = null)
    {
        return Use(new DelegateRule<double, TContext>((val, ctx) =>
        {
            if (val < 0) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "negative", message ?? "Must be negative"));
        }));
    }

    public DoubleSchema<TContext> Finite(string? message = null)
    {
        return Use(new DelegateRule<double, TContext>((val, ctx) =>
        {
            if (double.IsFinite(val)) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "finite", message ?? "Must be a finite number"));
        }));
    }

    public DoubleSchema<TContext> Refine(Func<double, TContext, bool> predicate, string message, string code = "custom_error")
    {
        return Use(new DelegateRule<double, TContext>((val, ctx) =>
        {
            if (predicate(val, ctx.Data)) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(ctx.Execution.Path, code, message));
        }));
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
    public Task<Result<double>> ValidateAsync(double value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        return ValidateAsync(value, context);
    }
}
