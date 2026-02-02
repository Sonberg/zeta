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
        Use(new StatefulRefinementRule<double, (double, string?)>(
            static (val, exec, state) => NumericValidators.Min(val, state.Item1, exec.Path, state.Item2),
            (min, message)));
        return this;
    }

    public DoubleContextlessSchema Max(double max, string? message = null)
    {
        Use(new StatefulRefinementRule<double, (double, string?)>(
            static (val, exec, state) => NumericValidators.Max(val, state.Item1, exec.Path, state.Item2),
            (max, message)));
        return this;
    }

    public DoubleContextlessSchema Positive(string? message = null)
    {
        Use(new StatefulRefinementRule<double, string?>(
            static (val, exec, state) =>
                val > 0
                    ? null
                    : new ValidationError(exec.Path, "positive", state ?? "Must be positive"),
            message));
        return this;
    }

    public DoubleContextlessSchema Negative(string? message = null)
    {
        Use(new StatefulRefinementRule<double, string?>(
            static (val, exec, state) =>
                val < 0
                    ? null
                    : new ValidationError(exec.Path, "negative", state ?? "Must be negative"),
            message));
        return this;
    }

    public DoubleContextlessSchema Finite(string? message = null)
    {
        Use(new StatefulRefinementRule<double, string?>(
            static (val, exec, state) =>
                !double.IsNaN(val) && !double.IsInfinity(val)
                    ? null
                    : new ValidationError(exec.Path, "finite", state ?? "Must be a finite number"),
            message));
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
