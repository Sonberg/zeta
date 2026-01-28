using Zeta.Core;
using Zeta.Rules;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating decimal values.
/// </summary>
public class DecimalContextSchema<TContext> : ContextSchema<decimal, TContext>
{
    public DecimalContextSchema() { }

    public DecimalContextSchema(ContextRuleEngine<decimal, TContext> rules) : base(rules) { }

    public DecimalContextSchema<TContext> Min(decimal min, string? message = null)
    {
        Use(new RefinementRule<decimal, TContext>((val, ctx) =>
            NumericValidators.Min(val, min, ctx.Path, message)));
        return this;
    }

    public DecimalContextSchema<TContext> Max(decimal max, string? message = null)
    {
        Use(new RefinementRule<decimal, TContext>((val, ctx) =>
            NumericValidators.Max(val, max, ctx.Path, message)));
        return this;
    }

    public DecimalContextSchema<TContext> Positive(string? message = null)
    {
        Use(new RefinementRule<decimal, TContext>((val, ctx) =>
            val > 0
                ? null
                : new ValidationError(ctx.Path, "positive", message ?? "Must be positive")));
        return this;
    }

    public DecimalContextSchema<TContext> Negative(string? message = null)
    {
        Use(new RefinementRule<decimal, TContext>((val, ctx) =>
            val < 0
                ? null
                : new ValidationError(ctx.Path, "negative", message ?? "Must be negative")));
        return this;
    }

    public DecimalContextSchema<TContext> Precision(int maxDecimalPlaces, string? message = null)
    {
        Use(new RefinementRule<decimal, TContext>((val, ctx) =>
            DecimalContextlessSchema.GetDecimalPlaces(val) <= maxDecimalPlaces
                ? null
                : new ValidationError(ctx.Path, "precision", message ?? $"Must have at most {maxDecimalPlaces} decimal places")));
        return this;
    }

    public DecimalContextSchema<TContext> MultipleOf(decimal step, string? message = null)
    {
        Use(new RefinementRule<decimal, TContext>((val, ctx) =>
            val % step == 0
                ? null
                : new ValidationError(ctx.Path, "multiple_of", message ?? $"Must be a multiple of {step}")));
        return this;
    }

    public DecimalContextSchema<TContext> Refine(Func<decimal, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<decimal, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public DecimalContextSchema<TContext> Refine(Func<decimal, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }

    public DecimalContextSchema<TContext> RefineAsync(
        Func<decimal, TContext, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        Use(new RefinementRule<decimal, TContext>(async (val, ctx) =>
            await predicate(val, ctx.Data, ctx.CancellationToken)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public DecimalContextSchema<TContext> RefineAsync(
        Func<decimal, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        return RefineAsync((val, _, ct) => predicate(val, ct), message, code);
    }
}
