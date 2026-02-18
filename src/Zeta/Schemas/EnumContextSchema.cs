using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Enum;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating enum values.
/// </summary>
public class EnumContextSchema<TEnum, TContext> : ContextSchema<TEnum, TContext, EnumContextSchema<TEnum, TContext>>
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

    public EnumContextSchema<TEnum, TContext> Defined(string? message = null)
        => Append(new DefinedRule<TEnum, TContext>(message));

    public EnumContextSchema<TEnum, TContext> OneOf(params TEnum[] allowed)
        => OneOf((IReadOnlyCollection<TEnum>)allowed, null);

    public EnumContextSchema<TEnum, TContext> OneOf(IReadOnlyCollection<TEnum> allowed, string? message = null)
        => Append(new OneOfRule<TEnum, TContext>(allowed, message));
}

