using Zeta.Core;
using Zeta.Rules;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating integer values.
/// </summary>
public class IntContextSchema<TContext> : ContextSchema<int, TContext>
{
    public IntContextSchema() { }

    public IntContextSchema(ContextRuleEngine<int, TContext> rules) : base(rules) { }

    public IntContextSchema<TContext> Min(int min, string? message = null)
    {
        Use(new RefinementRule<int, TContext>((val, ctx) =>
            NumericValidators.Min(val, min, ctx.Path, message)));
        return this;
    }

    public IntContextSchema<TContext> Max(int max, string? message = null)
    {
        Use(new RefinementRule<int, TContext>((val, ctx) =>
            NumericValidators.Max(val, max, ctx.Path, message)));
        return this;
    }

    public IntContextSchema<TContext> Refine(Func<int, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<int, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public IntContextSchema<TContext> Refine(Func<int, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}
