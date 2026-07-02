using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating decimal values.
/// </summary>
public class DecimalContextSchema<TContext> : ContextSchema<decimal, TContext, DecimalContextSchema<TContext>>, IValueSchema<decimal, DecimalContextSchema<TContext>>
{
    internal DecimalContextSchema() { }

    internal DecimalContextSchema(ContextRuleEngine<decimal, TContext> rules) : base(rules)
    {
    }

    private DecimalContextSchema(
        ContextRuleEngine<decimal, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<decimal, TContext>>? conditionals,
        Func<decimal, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
    }

    protected override DecimalContextSchema<TContext> CreateInstance() => new();

    private protected override DecimalContextSchema<TContext> CreateInstance(
        ContextRuleEngine<decimal, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<decimal, TContext>>? conditionals,
        Func<decimal, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, allowNull, conditionals, contextFactory);
}
