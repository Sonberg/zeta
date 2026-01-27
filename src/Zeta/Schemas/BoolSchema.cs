using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating boolean values.
/// </summary>
public sealed class BoolSchema : ContextlessSchema<bool>
{
    public BoolSchema() { }

    public BoolSchema IsTrue(string? message = null)
    {
        Use(new RefinementRule<bool>((val, exec) =>
            val
                ? null
                : new ValidationError(exec.Path, "is_true", message ?? "Must be true")));
        return this;
    }

    public BoolSchema IsFalse(string? message = null)
    {
        Use(new RefinementRule<bool>((val, exec) =>
            !val
                ? null
                : new ValidationError(exec.Path, "is_false", message ?? "Must be false")));
        return this;
    }

    public BoolSchema Refine(Func<bool, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<bool>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    /// <summary>
    /// Creates a context-aware bool schema with all rules from this schema.
    /// </summary>
    public BoolSchema<TContext> WithContext<TContext>()
        => new BoolSchema<TContext>(Rules.ToContext<TContext>());
}

/// <summary>
/// A context-aware schema for validating boolean values.
/// </summary>
public class BoolSchema<TContext> : ContextSchema<bool, TContext>
{
    public BoolSchema() { }

    public BoolSchema(ContextRuleEngine<bool, TContext> rules) : base(rules) { }

    public BoolSchema<TContext> IsTrue(string? message = null)
    {
        Use(new RefinementRule<bool, TContext>((val, ctx) =>
            val
                ? null
                : new ValidationError(ctx.Execution.Path, "is_true", message ?? "Must be true")));
        return this;
    }

    public BoolSchema<TContext> IsFalse(string? message = null)
    {
        Use(new RefinementRule<bool, TContext>((val, ctx) =>
            !val
                ? null
                : new ValidationError(ctx.Execution.Path, "is_false", message ?? "Must be false")));
        return this;
    }

    public BoolSchema<TContext> Refine(Func<bool, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<bool, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
    }

    public BoolSchema<TContext> Refine(Func<bool, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}
