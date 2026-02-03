using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating boolean values.
/// </summary>
public sealed class BoolContextlessSchema : ContextlessSchema<bool>
{
    public BoolContextlessSchema() { }

    public BoolContextlessSchema IsTrue(string? message = null)
    {
        Use(new RefinementRule<bool>((val, exec) =>
            val
                ? null
                : new ValidationError(exec.Path, "is_true", message ?? "Must be true")));
        return this;
    }

    public BoolContextlessSchema IsFalse(string? message = null)
    {
        Use(new RefinementRule<bool>((val, exec) =>
            !val
                ? null
                : new ValidationError(exec.Path, "is_false", message ?? "Must be false")));
        return this;
    }

    public BoolContextlessSchema Refine(Func<bool, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<bool>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    public BoolContextlessSchema RefineAsync(
        Func<bool, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        Use(new RefinementRule<bool>(async (val, exec) =>
            await predicate(val, exec.CancellationToken)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    public BoolContextlessSchema If(
        Func<bool, bool> condition,
        Func<BoolContextlessSchema, BoolContextlessSchema> configure)
    {
        var inner = configure(new BoolContextlessSchema());
        foreach (var rule in inner.Rules.GetRules())
            Use(new ConditionalRule<bool>(condition, rule));
        return this;
    }

    /// <summary>
    /// Creates a context-aware bool schema with all rules from this schema.
    /// </summary>
    public BoolContextSchema<TContext> WithContext<TContext>()
        => new BoolContextSchema<TContext>(Rules.ToContext<TContext>());
}
