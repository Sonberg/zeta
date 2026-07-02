using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating string values.
/// </summary>
public class StringContextSchema<TContext> : ContextSchema<string, TContext, StringContextSchema<TContext>>, IValueSchema<string, StringContextSchema<TContext>>
{
    internal StringContextSchema() { }

    internal StringContextSchema(ContextRuleEngine<string, TContext> rules) : base(rules)
    {
    }

    private StringContextSchema(
        ContextRuleEngine<string, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<string, TContext>>? conditionals,
        Func<string, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
    }

    protected override StringContextSchema<TContext> CreateInstance() => new();

    private protected override StringContextSchema<TContext> CreateInstance(
        ContextRuleEngine<string, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<string, TContext>>? conditionals,
        Func<string, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, allowNull, conditionals, contextFactory);
}
