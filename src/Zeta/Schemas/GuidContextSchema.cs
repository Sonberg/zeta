using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating Guid values.
/// </summary>
public class GuidContextSchema<TContext> : ContextSchema<Guid, TContext, GuidContextSchema<TContext>>, IValueSchema<Guid, GuidContextSchema<TContext>>
{
    internal GuidContextSchema() { }

    internal GuidContextSchema(ContextRuleEngine<Guid, TContext> rules) : base(rules)
    {
    }

    private GuidContextSchema(
        ContextRuleEngine<Guid, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<Guid, TContext>>? conditionals,
        Func<Guid, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
    }

    protected override GuidContextSchema<TContext> CreateInstance() => new();

    private protected override GuidContextSchema<TContext> CreateInstance(
        ContextRuleEngine<Guid, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<Guid, TContext>>? conditionals,
        Func<Guid, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, allowNull, conditionals, contextFactory);
}
