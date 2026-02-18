using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Enum;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating enum values.
/// </summary>
public sealed class EnumContextlessSchema<TEnum> : ContextlessSchema<TEnum, EnumContextlessSchema<TEnum>>
    where TEnum : struct, Enum
{
    internal EnumContextlessSchema()
    {
    }

    private EnumContextlessSchema(
        ContextlessRuleEngine<TEnum> rules,
        bool allowNull,
        IReadOnlyList<(Func<TEnum, bool>, ISchema<TEnum>)>? conditionals)
        : base(rules, allowNull, conditionals)
    {
    }

    protected override EnumContextlessSchema<TEnum> CreateInstance() => new();

    protected override EnumContextlessSchema<TEnum> CreateInstance(
        ContextlessRuleEngine<TEnum> rules,
        bool allowNull,
        IReadOnlyList<(Func<TEnum, bool>, ISchema<TEnum>)>? conditionals)
        => new(rules, allowNull, conditionals);

    public EnumContextlessSchema<TEnum> Defined(string? message = null)
        => Append(new DefinedRule<TEnum>(message));

    public EnumContextlessSchema<TEnum> OneOf(params TEnum[] allowed)
        => OneOf((IReadOnlyCollection<TEnum>)allowed, null);

    public EnumContextlessSchema<TEnum> OneOf(IReadOnlyCollection<TEnum> allowed, string? message = null)
        => Append(new OneOfRule<TEnum>(allowed, message));

    /// <summary>
    /// Creates a context-aware enum schema with all rules from this schema.
    /// </summary>
    public EnumContextSchema<TEnum, TContext> Using<TContext>()
    {
        var schema = new EnumContextSchema<TEnum, TContext>(Rules.ToContext<TContext>());
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware enum schema with a factory delegate for creating context data.
    /// </summary>
    public EnumContextSchema<TEnum, TContext> Using<TContext>(
        Func<TEnum, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }
}

