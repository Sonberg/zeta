using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating boolean values.
/// </summary>
public class BoolContextSchema<TContext> : ContextSchema<bool, TContext, BoolContextSchema<TContext>>
{
    internal BoolContextSchema() { }

    internal BoolContextSchema(ContextRuleEngine<bool, TContext> rules) : base(rules) { }

    protected override BoolContextSchema<TContext> CreateInstance() => new();

    public BoolContextSchema<TContext> IsTrue(string? message = null)
    {
        Use(new RefinementRule<bool, TContext>((val, ctx) =>
            val
                ? null
                : new ValidationError(ctx.Path, "is_true", message ?? "Must be true")));
        return this;
    }

    public BoolContextSchema<TContext> IsFalse(string? message = null)
    {
        Use(new RefinementRule<bool, TContext>((val, ctx) =>
            !val
                ? null
                : new ValidationError(ctx.Path, "is_false", message ?? "Must be false")));
        return this;
    }
}
