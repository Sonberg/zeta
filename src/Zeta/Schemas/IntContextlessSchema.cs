using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Numeric;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating integer values.
/// </summary>
public sealed class IntContextlessSchema : ContextlessSchema<int>
{
    public IntContextlessSchema() { }

    public IntContextlessSchema Min(int min, string? message = null)
    {
        Use(new MinIntRule(min, message));
        return this;
    }

    public IntContextlessSchema Max(int max, string? message = null)
    {
        Use(new MaxIntRule(max, message));
        return this;
    }

    public IntContextlessSchema Refine(Func<int, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<int>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    public IntContextlessSchema RefineAsync(
        Func<int, CancellationToken, ValueTask<bool>> predicate,
        string message,
        string code = "custom_error")
    {
        Use(new RefinementRule<int>(async (val, exec) =>
            await predicate(val, exec.CancellationToken)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    public IntContextlessSchema If(
        Func<int, bool> condition,
        Func<IntContextlessSchema, IntContextlessSchema> configure)
    {
        var inner = configure(new IntContextlessSchema());
        foreach (var rule in inner.Rules.GetRules())
            Use(new ConditionalRule<int>(condition, rule));
        return this;
    }

    /// <summary>
    /// Creates a context-aware int schema with all rules from this schema.
    /// </summary>
    public IntContextSchema<TContext> WithContext<TContext>()
        => new IntContextSchema<TContext>(Rules.ToContext<TContext>());
}
