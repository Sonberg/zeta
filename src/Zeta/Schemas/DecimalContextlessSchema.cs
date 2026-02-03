using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Numeric;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating decimal values.
/// </summary>
public sealed class DecimalContextlessSchema : ContextlessSchema<decimal>
{
    public DecimalContextlessSchema() { }

    public DecimalContextlessSchema Min(decimal min, string? message = null)
    {
        Use(new MinDecimalRule(min, message));
        return this;
    }

    public DecimalContextlessSchema Max(decimal max, string? message = null)
    {
        Use(new MaxDecimalRule(max, message));
        return this;
    }

    public DecimalContextlessSchema Positive(string? message = null)
    {
        Use(new PositiveDecimalRule(message));
        return this;
    }

    public DecimalContextlessSchema Negative(string? message = null)
    {
        Use(new NegativeDecimalRule(message));
        return this;
    }

    public DecimalContextlessSchema Precision(int maxDecimalPlaces, string? message = null)
    {
        Use(new PrecisionRule(maxDecimalPlaces, message));
        return this;
    }

    public DecimalContextlessSchema MultipleOf(decimal step, string? message = null)
    {
        Use(new MultipleOfRule(step, message));
        return this;
    }

    public DecimalContextlessSchema Refine(Func<decimal, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<decimal>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    public DecimalContextlessSchema RefineAsync(
        Func<decimal, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        Use(new RefinementRule<decimal>(async (val, exec) =>
            await predicate(val, exec.CancellationToken)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    /// <summary>
    /// Creates a context-aware decimal schema with all rules from this schema.
    /// </summary>
    public DecimalContextSchema<TContext> WithContext<TContext>()
        => new DecimalContextSchema<TContext>(Rules.ToContext<TContext>());
}
