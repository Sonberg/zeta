using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating decimal values.
/// </summary>
public sealed class DecimalContextlessSchema : ContextlessSchema<decimal, DecimalContextlessSchema>, IValueSchema<decimal, DecimalContextlessSchema>
{
    internal DecimalContextlessSchema()
    {
    }

    private DecimalContextlessSchema(
        ContextlessRuleEngine<decimal> rules,
        bool allowNull,
        IReadOnlyList<(Func<decimal, bool>, ISchema<decimal>)>? conditionals)
        : base(rules, allowNull, conditionals)
    {
    }

    protected override DecimalContextlessSchema CreateInstance() => new();

    protected override DecimalContextlessSchema CreateInstance(
        ContextlessRuleEngine<decimal> rules,
        bool allowNull,
        IReadOnlyList<(Func<decimal, bool>, ISchema<decimal>)>? conditionals)
        => new(rules, allowNull, conditionals);

    /// <summary>
    /// Creates a context-aware decimal schema with all rules from this schema.
    /// </summary>
    public DecimalContextSchema<TContext> Using<TContext>()
    {
        var schema = new DecimalContextSchema<TContext>(Rules.ToContext<TContext>());
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware decimal schema with a factory delegate for creating context data.
    /// </summary>
    public DecimalContextSchema<TContext> Using<TContext>(
        Func<decimal, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }
}
