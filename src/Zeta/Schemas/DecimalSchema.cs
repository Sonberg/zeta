using Zeta.Core;
using Zeta.Rules;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating decimal values.
/// </summary>
public sealed class DecimalSchema : ISchema<decimal>
{
    private readonly RuleEngine<decimal> _rules = new();

    public async ValueTask<Result<decimal>> ValidateAsync(decimal value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var errors = await _rules.ExecuteAsync(value, execution);

        return errors == null
            ? Result<decimal>.Success(value)
            : Result<decimal>.Failure(errors);
    }

    public DecimalSchema Min(decimal min, string? message = null)
    {
        _rules.Add(new RefinementRule<decimal>((val, exec) =>
            NumericValidators.Min(val, min, exec.Path, message)));
        return this;
    }

    public DecimalSchema Max(decimal max, string? message = null)
    {
        _rules.Add(new RefinementRule<decimal>((val, exec) =>
            NumericValidators.Max(val, max, exec.Path, message)));
        return this;
    }

    public DecimalSchema Positive(string? message = null)
    {
        _rules.Add(new RefinementRule<decimal>((val, exec) =>
            val > 0
                ? null
                : new ValidationError(exec.Path, "positive", message ?? "Must be positive")));
        return this;
    }

    public DecimalSchema Negative(string? message = null)
    {
        _rules.Add(new RefinementRule<decimal>((val, exec) =>
            val < 0
                ? null
                : new ValidationError(exec.Path, "negative", message ?? "Must be negative")));
        return this;
    }

    public DecimalSchema Precision(int maxDecimalPlaces, string? message = null)
    {
        _rules.Add(new RefinementRule<decimal>((val, exec) =>
            GetDecimalPlaces(val) <= maxDecimalPlaces
                ? null
                : new ValidationError(exec.Path, "precision", message ?? $"Must have at most {maxDecimalPlaces} decimal places")));
        return this;
    }

    public DecimalSchema MultipleOf(decimal step, string? message = null)
    {
        _rules.Add(new RefinementRule<decimal>((val, exec) =>
            val % step == 0
                ? null
                : new ValidationError(exec.Path, "multiple_of", message ?? $"Must be a multiple of {step}")));
        return this;
    }

    public DecimalSchema Refine(Func<decimal, bool> predicate, string message, string code = "custom_error")
    {
        _rules.Add(new RefinementRule<decimal>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    private static int GetDecimalPlaces(decimal value)
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
}

/// <summary>
/// A context-aware schema for validating decimal values.
/// </summary>
public class DecimalSchema<TContext> : BaseSchema<decimal, TContext>
{
    public DecimalSchema<TContext> Min(decimal min, string? message = null)
    {
        Use(new RefinementRule<decimal, TContext>((val, ctx) =>
            NumericValidators.Min(val, min, ctx.Execution.Path, message)));
        return this;
    }

    public DecimalSchema<TContext> Max(decimal max, string? message = null)
    {
        Use(new RefinementRule<decimal, TContext>((val, ctx) =>
            NumericValidators.Max(val, max, ctx.Execution.Path, message)));
        return this;
    }

    public DecimalSchema<TContext> Positive(string? message = null)
    {
        Use(new RefinementRule<decimal, TContext>((val, ctx) =>
            val > 0
                ? null
                : new ValidationError(ctx.Execution.Path, "positive", message ?? "Must be positive")));
        return this;
    }

    public DecimalSchema<TContext> Negative(string? message = null)
    {
        Use(new RefinementRule<decimal, TContext>((val, ctx) =>
            val < 0
                ? null
                : new ValidationError(ctx.Execution.Path, "negative", message ?? "Must be negative")));
        return this;
    }

    public DecimalSchema<TContext> Precision(int maxDecimalPlaces, string? message = null)
    {
        Use(new RefinementRule<decimal, TContext>((val, ctx) =>
            GetDecimalPlaces(val) <= maxDecimalPlaces
                ? null
                : new ValidationError(ctx.Execution.Path, "precision", message ?? $"Must have at most {maxDecimalPlaces} decimal places")));
        return this;
    }

    public DecimalSchema<TContext> MultipleOf(decimal step, string? message = null)
    {
        Use(new RefinementRule<decimal, TContext>((val, ctx) =>
            val % step == 0
                ? null
                : new ValidationError(ctx.Execution.Path, "multiple_of", message ?? $"Must be a multiple of {step}")));
        return this;
    }

    public DecimalSchema<TContext> Refine(Func<decimal, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<decimal, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
    }

    public DecimalSchema<TContext> Refine(Func<decimal, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }

    private static int GetDecimalPlaces(decimal value)
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
}
