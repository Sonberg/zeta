using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating DateTime values.
/// </summary>
public class DateTimeContextSchema<TContext> : ContextSchema<DateTime, TContext, DateTimeContextSchema<TContext>>, IValueSchema<DateTime, DateTimeContextSchema<TContext>>
{
    internal DateTimeContextSchema() { }

    internal DateTimeContextSchema(ContextRuleEngine<DateTime, TContext> rules) : base(rules)
    {
    }

    private DateTimeContextSchema(
        ContextRuleEngine<DateTime, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<DateTime, TContext>>? conditionals,
        Func<DateTime, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
    }

    protected override DateTimeContextSchema<TContext> CreateInstance() => new();

    private protected override DateTimeContextSchema<TContext> CreateInstance(
        ContextRuleEngine<DateTime, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<DateTime, TContext>>? conditionals,
        Func<DateTime, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, allowNull, conditionals, contextFactory);
}
