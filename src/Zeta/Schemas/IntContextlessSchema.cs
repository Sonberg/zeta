using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Numeric;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating integer values.
/// </summary>
public sealed class IntContextlessSchema : ContextlessSchema<int, IntContextlessSchema>
{
    internal IntContextlessSchema()
    {
    }

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

    /// <summary>
    /// Creates a context-aware int schema with all rules from this schema.
    /// </summary>
    public IntContextSchema<TContext> WithContext<TContext>() => new(Rules.ToContext<TContext>());
}