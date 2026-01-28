using Zeta.Core;
using Zeta.Rules;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating double values.
/// </summary>
public class DoubleContextSchema<TContext> : ContextSchema<double, TContext>
{
    public DoubleContextSchema() { }

    public DoubleContextSchema(ContextRuleEngine<double, TContext> rules) : base(rules) { }

    public DoubleContextSchema<TContext> Min(double min, string? message = null)
    {
        Use(new RefinementRule<double, TContext>((val, ctx) =>
            NumericValidators.Min(val, min, ctx.Path, message)));
        return this;
    }

    public DoubleContextSchema<TContext> Max(double max, string? message = null)
    {
        Use(new RefinementRule<double, TContext>((val, ctx) =>
            NumericValidators.Max(val, max, ctx.Path, message)));
        return this;
    }

    public DoubleContextSchema<TContext> Positive(string? message = null)
    {
        Use(new RefinementRule<double, TContext>((val, ctx) =>
            val > 0
                ? null
                : new ValidationError(ctx.Path, "positive", message ?? "Must be positive")));
        return this;
    }

    public DoubleContextSchema<TContext> Negative(string? message = null)
    {
        Use(new RefinementRule<double, TContext>((val, ctx) =>
            val < 0
                ? null
                : new ValidationError(ctx.Path, "negative", message ?? "Must be negative")));
        return this;
    }

    public DoubleContextSchema<TContext> Finite(string? message = null)
    {
        Use(new RefinementRule<double, TContext>((val, ctx) =>
            !double.IsNaN(val) && !double.IsInfinity(val)
                ? null
                : new ValidationError(ctx.Path, "finite", message ?? "Must be a finite number")));
        return this;
    }

    public DoubleContextSchema<TContext> Refine(Func<double, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<double, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public DoubleContextSchema<TContext> Refine(Func<double, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }

    public DoubleContextSchema<TContext> RefineAsync(
        Func<double, TContext, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        Use(new RefinementRule<double, TContext>(async (val, ctx) =>
            await predicate(val, ctx.Data, ctx.CancellationToken)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public DoubleContextSchema<TContext> RefineAsync(
        Func<double, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        return RefineAsync((val, _, ct) => predicate(val, ct), message, code);
    }
}
