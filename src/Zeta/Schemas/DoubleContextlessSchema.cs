using Zeta.Core;
using Zeta.Rules;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating double values.
/// </summary>
public sealed class DoubleContextlessSchema : ContextlessSchema<double>
{
    public DoubleContextlessSchema() { }

    public DoubleContextlessSchema Min(double min, string? message = null)
    {
        Use(new RefinementRule<double>((val, exec) =>
            NumericValidators.Min(val, min, exec.Path, message)));
        return this;
    }

    public DoubleContextlessSchema Max(double max, string? message = null)
    {
        Use(new RefinementRule<double>((val, exec) =>
            NumericValidators.Max(val, max, exec.Path, message)));
        return this;
    }

    public DoubleContextlessSchema Positive(string? message = null)
    {
        Use(new RefinementRule<double>((val, exec) =>
            val > 0
                ? null
                : new ValidationError(exec.Path, "positive", message ?? "Must be positive")));
        return this;
    }

    public DoubleContextlessSchema Negative(string? message = null)
    {
        Use(new RefinementRule<double>((val, exec) =>
            val < 0
                ? null
                : new ValidationError(exec.Path, "negative", message ?? "Must be negative")));
        return this;
    }

    public DoubleContextlessSchema Finite(string? message = null)
    {
        Use(new RefinementRule<double>((val, exec) =>
            !double.IsNaN(val) && !double.IsInfinity(val)
                ? null
                : new ValidationError(exec.Path, "finite", message ?? "Must be a finite number")));
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

    /// <summary>
    /// Creates a context-aware double schema with all rules from this schema.
    /// </summary>
    public DoubleContextSchema<TContext> WithContext<TContext>()
        => new DoubleContextSchema<TContext>(Rules.ToContext<TContext>());
}
