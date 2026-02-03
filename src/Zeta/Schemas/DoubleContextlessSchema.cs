using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Numeric;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating double values.
/// </summary>
public sealed class DoubleContextlessSchema : ContextlessSchema<double>
{
    public DoubleContextlessSchema() { }

    public DoubleContextlessSchema Min(double min, string? message = null)
    {
        Use(new MinDoubleRule(min, message));
        return this;
    }

    public DoubleContextlessSchema Max(double max, string? message = null)
    {
        Use(new MaxDoubleRule(max, message));
        return this;
    }

    public DoubleContextlessSchema Positive(string? message = null)
    {
        Use(new PositiveDoubleRule(message));
        return this;
    }

    public DoubleContextlessSchema Negative(string? message = null)
    {
        Use(new NegativeDoubleRule(message));
        return this;
    }

    public DoubleContextlessSchema Finite(string? message = null)
    {
        Use(new FiniteRule(message));
        return this;
    }

    public DoubleContextlessSchema Refine(Func<double, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<double>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    public DoubleContextlessSchema RefineAsync(
        Func<double, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        Use(new RefinementRule<double>(async (val, exec) =>
            await predicate(val, exec.CancellationToken)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    /// <summary>
    /// Creates a context-aware double schema with all rules from this schema.
    /// </summary>
    public DoubleContextSchema<TContext> WithContext<TContext>()
        => new DoubleContextSchema<TContext>(Rules.ToContext<TContext>());
}
