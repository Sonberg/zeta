#if !NETSTANDARD2_0
using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating TimeOnly values.
/// </summary>
public class TimeOnlyContextSchema<TContext> : ContextSchema<TimeOnly, TContext, TimeOnlyContextSchema<TContext>>, IValueSchema<TimeOnly, TimeOnlyContextSchema<TContext>>
{
    internal TimeOnlyContextSchema() { }

    internal TimeOnlyContextSchema(ContextRuleEngine<TimeOnly, TContext> rules) : base(rules)
    {
    }

    private TimeOnlyContextSchema(
        ContextRuleEngine<TimeOnly, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<TimeOnly, TContext>>? conditionals,
        Func<TimeOnly, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
    }

    protected override TimeOnlyContextSchema<TContext> CreateInstance() => new();

    private protected override TimeOnlyContextSchema<TContext> CreateInstance(
        ContextRuleEngine<TimeOnly, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<TimeOnly, TContext>>? conditionals,
        Func<TimeOnly, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, allowNull, conditionals, contextFactory);
}
#endif
