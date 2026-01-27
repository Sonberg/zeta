using Zeta.Core;
using Zeta.Rules;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating double values.
/// </summary>
public sealed class DoubleSchema : ContextlessSchema<double>
{
    public DoubleSchema Min(double min, string? message = null)
    {
        Use(new RefinementRule<double>((val, exec) =>
            NumericValidators.Min(val, min, exec.Path, message)));
        return this;
    }

    public DoubleSchema Max(double max, string? message = null)
    {
        Use(new RefinementRule<double>((val, exec) =>
            NumericValidators.Max(val, max, exec.Path, message)));
        return this;
    }

    public DoubleSchema Positive(string? message = null)
    {
        Use(new RefinementRule<double>((val, exec) =>
            val > 0
                ? null
                : new ValidationError(exec.Path, "positive", message ?? "Must be positive")));
        return this;
    }

    public DoubleSchema Negative(string? message = null)
    {
        Use(new RefinementRule<double>((val, exec) =>
            val < 0
                ? null
                : new ValidationError(exec.Path, "negative", message ?? "Must be negative")));
        return this;
    }

    public DoubleSchema Finite(string? message = null)
    {
        Use(new RefinementRule<double>((val, exec) =>
            !double.IsNaN(val) && !double.IsInfinity(val)
                ? null
                : new ValidationError(exec.Path, "finite", message ?? "Must be a finite number")));
        return this;
    }

    public DoubleSchema Refine(Func<double, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<double>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }
}

/// <summary>
/// A context-aware schema for validating double values.
/// </summary>
public class DoubleSchema<TContext> : ContextSchema<double, TContext>
{
    public DoubleSchema<TContext> Min(double min, string? message = null)
    {
        Use(new RefinementRule<double, TContext>((val, ctx) =>
            NumericValidators.Min(val, min, ctx.Execution.Path, message)));
        return this;
    }

    public DoubleSchema<TContext> Max(double max, string? message = null)
    {
        Use(new RefinementRule<double, TContext>((val, ctx) =>
            NumericValidators.Max(val, max, ctx.Execution.Path, message)));
        return this;
    }

    public DoubleSchema<TContext> Positive(string? message = null)
    {
        Use(new RefinementRule<double, TContext>((val, ctx) =>
            val > 0
                ? null
                : new ValidationError(ctx.Execution.Path, "positive", message ?? "Must be positive")));
        return this;
    }

    public DoubleSchema<TContext> Negative(string? message = null)
    {
        Use(new RefinementRule<double, TContext>((val, ctx) =>
            val < 0
                ? null
                : new ValidationError(ctx.Execution.Path, "negative", message ?? "Must be negative")));
        return this;
    }

    public DoubleSchema<TContext> Finite(string? message = null)
    {
        Use(new RefinementRule<double, TContext>((val, ctx) =>
            !double.IsNaN(val) && !double.IsInfinity(val)
                ? null
                : new ValidationError(ctx.Execution.Path, "finite", message ?? "Must be a finite number")));
        return this;
    }

    public DoubleSchema<TContext> Refine(Func<double, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<double, TContext>((val, ctx) =>
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
