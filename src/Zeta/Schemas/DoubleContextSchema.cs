using Zeta.Core;
using Zeta.Rules.Numeric;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating double values.
/// </summary>
public class DoubleContextSchema<TContext> : ContextSchema<double, TContext, DoubleContextSchema<TContext>>
{
    internal DoubleContextSchema() { }

    internal DoubleContextSchema(ContextRuleEngine<double, TContext> rules) : base(rules)
    {
    }

    private DoubleContextSchema(
        ContextRuleEngine<double, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<double, TContext>>? conditionals,
        Func<double, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
    }

    protected override DoubleContextSchema<TContext> CreateInstance() => new();

    private protected override DoubleContextSchema<TContext> CreateInstance(
        ContextRuleEngine<double, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<double, TContext>>? conditionals,
        Func<double, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, allowNull, conditionals, contextFactory);

    public DoubleContextSchema<TContext> Min(double min, string? message = null)
        => Append(new MinDoubleRule<TContext>(min, message));

    public DoubleContextSchema<TContext> Max(double max, string? message = null)
        => Append(new MaxDoubleRule<TContext>(max, message));

    public DoubleContextSchema<TContext> Positive(string? message = null)
        => Append(new PositiveDoubleRule<TContext>(message));

    public DoubleContextSchema<TContext> Negative(string? message = null)
        => Append(new NegativeDoubleRule<TContext>(message));

    public DoubleContextSchema<TContext> Finite(string? message = null)
        => Append(new FiniteRule<TContext>(message));
}
