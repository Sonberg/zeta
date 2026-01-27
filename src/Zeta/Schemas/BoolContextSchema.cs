using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating boolean values.
/// </summary>
public class BoolContextSchema<TContext> : ContextSchema<bool, TContext>
{
    public BoolContextSchema() { }

    public BoolContextSchema(ContextRuleEngine<bool, TContext> rules) : base(rules) { }

    public BoolContextSchema<TContext> IsTrue(string? message = null)
    {
        Use(new RefinementRule<bool, TContext>((val, ctx) =>
            val
                ? null
                : new ValidationError(ctx.Execution.Path, "is_true", message ?? "Must be true")));
        return this;
    }

    public BoolContextSchema<TContext> IsFalse(string? message = null)
    {
        Use(new RefinementRule<bool, TContext>((val, ctx) =>
            !val
                ? null
                : new ValidationError(ctx.Execution.Path, "is_false", message ?? "Must be false")));
        return this;
    }

    public BoolContextSchema<TContext> Refine(Func<bool, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<bool, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
    }

    public BoolContextSchema<TContext> Refine(Func<bool, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}
