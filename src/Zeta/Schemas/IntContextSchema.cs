using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Numeric;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating integer values.
/// </summary>
public class IntContextSchema<TContext> : ContextSchema<int, TContext, IntContextSchema<TContext>>
{
    internal IntContextSchema() { }

    internal IntContextSchema(ContextRuleEngine<int, TContext> rules) : base(rules) { }

    protected override IntContextSchema<TContext> CreateInstance() => new();

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
}
