#if !NETSTANDARD2_0
using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating DateOnly values.
/// </summary>
public class DateOnlyContextSchema<TContext> : ContextSchema<DateOnly, TContext, DateOnlyContextSchema<TContext>>, IValueSchema<DateOnly, DateOnlyContextSchema<TContext>>
{
    internal DateOnlyContextSchema() { }

    internal DateOnlyContextSchema(ContextRuleEngine<DateOnly, TContext> rules) : base(rules)
    {
    }

    private DateOnlyContextSchema(
        ContextRuleEngine<DateOnly, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<DateOnly, TContext>>? conditionals,
        Func<DateOnly, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
    }

    protected override DateOnlyContextSchema<TContext> CreateInstance() => new();

    private protected override DateOnlyContextSchema<TContext> CreateInstance(
        ContextRuleEngine<DateOnly, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<DateOnly, TContext>>? conditionals,
        Func<DateOnly, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, allowNull, conditionals, contextFactory);
}
#endif
