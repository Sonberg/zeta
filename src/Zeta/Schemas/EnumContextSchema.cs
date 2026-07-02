using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating enum values.
/// </summary>
public class EnumContextSchema<TEnum, TContext> : ContextSchema<TEnum, TContext, EnumContextSchema<TEnum, TContext>>, IValueSchema<TEnum, EnumContextSchema<TEnum, TContext>>
    where TEnum : struct, Enum
{
    internal EnumContextSchema()
    {
    }

    internal EnumContextSchema(ContextRuleEngine<TEnum, TContext> rules) : base(rules)
    {
    }

    private EnumContextSchema(
        ContextRuleEngine<TEnum, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<TEnum, TContext>>? conditionals,
        Func<TEnum, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
    }

    protected override EnumContextSchema<TEnum, TContext> CreateInstance() => new();

    private protected override EnumContextSchema<TEnum, TContext> CreateInstance(
        ContextRuleEngine<TEnum, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<TEnum, TContext>>? conditionals,
        Func<TEnum, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, allowNull, conditionals, contextFactory);
}
