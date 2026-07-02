using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating double values.
/// </summary>
public class DoubleContextSchema<TContext> : ContextSchema<double, TContext, DoubleContextSchema<TContext>>, IValueSchema<double, DoubleContextSchema<TContext>>
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
}
