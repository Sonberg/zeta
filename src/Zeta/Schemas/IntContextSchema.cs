using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Numeric;

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
        Use(new MinIntRule<TContext>(min, message));
        return this;
    }

    public IntContextSchema<TContext> Max(int max, string? message = null)
    {
        Use(new MaxIntRule<TContext>(max, message));
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

    public IntContextSchema<TContext> RefineAsync(
        Func<int, TContext, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        Use(new RefinementRule<int, TContext>(async (val, ctx) =>
            await predicate(val, ctx.Data, ctx.CancellationToken)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public IntContextSchema<TContext> RefineAsync(
        Func<int, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        return RefineAsync((val, _, ct) => predicate(val, ct), message, code);
    }

    public IntContextSchema<TContext> If(
        Func<int, TContext, bool> condition,
        Func<IntContextSchema<TContext>, IntContextSchema<TContext>> configure)
    {
        var inner = configure(new IntContextSchema<TContext>());
        foreach (var rule in inner.Rules.GetRules())
            Use(new ConditionalRule<int, TContext>(condition, rule));
        return this;
    }

    public IntContextSchema<TContext> If(
        Func<int, bool> condition,
        Func<IntContextSchema<TContext>, IntContextSchema<TContext>> configure)
        => If((val, _) => condition(val), configure);
}
