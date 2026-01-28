using Zeta.Core;
using Zeta.Rules;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating decimal values.
/// </summary>
public sealed class DecimalContextlessSchema : ContextlessSchema<decimal>
{
    public DecimalContextlessSchema() { }

    public DecimalContextlessSchema Min(decimal min, string? message = null)
    {
        Use(new RefinementRule<decimal>((val, exec) =>
            NumericValidators.Min(val, min, exec.Path, message)));
        return this;
    }

    public DecimalContextlessSchema Max(decimal max, string? message = null)
    {
        Use(new RefinementRule<decimal>((val, exec) =>
            NumericValidators.Max(val, max, exec.Path, message)));
        return this;
    }

    public DecimalContextlessSchema Positive(string? message = null)
    {
        Use(new RefinementRule<decimal>((val, exec) =>
            val > 0
                ? null
                : new ValidationError(exec.Path, "positive", message ?? "Must be positive")));
        return this;
    }

    public DecimalContextlessSchema Negative(string? message = null)
    {
        Use(new RefinementRule<decimal>((val, exec) =>
            val < 0
                ? null
                : new ValidationError(exec.Path, "negative", message ?? "Must be negative")));
        return this;
    }

    public DecimalContextlessSchema Precision(int maxDecimalPlaces, string? message = null)
    {
        Use(new RefinementRule<decimal>((val, exec) =>
            GetDecimalPlaces(val) <= maxDecimalPlaces
                ? null
                : new ValidationError(exec.Path, "precision", message ?? $"Must have at most {maxDecimalPlaces} decimal places")));
        return this;
    }

    public DecimalContextlessSchema MultipleOf(decimal step, string? message = null)
    {
        Use(new RefinementRule<decimal>((val, exec) =>
            val % step == 0
                ? null
                : new ValidationError(exec.Path, "multiple_of", message ?? $"Must be a multiple of {step}")));
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

    internal static int GetDecimalPlaces(decimal value)
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

    /// <summary>
    /// Creates a context-aware decimal schema with all rules from this schema.
    /// </summary>
    public DecimalContextSchema<TContext> WithContext<TContext>()
        => new DecimalContextSchema<TContext>(Rules.ToContext<TContext>());
}
