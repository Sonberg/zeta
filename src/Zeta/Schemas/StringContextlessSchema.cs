using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating string values.
/// </summary>
public sealed class StringContextlessSchema : ContextlessSchema<string, StringContextlessSchema>, IValueSchema<string, StringContextlessSchema>
{
    internal StringContextlessSchema()
    {
    }

    private StringContextlessSchema(
        ContextlessRuleEngine<string> rules,
        bool allowNull,
        IReadOnlyList<(Func<string, bool>, ISchema<string>)>? conditionals)
        : base(rules, allowNull, conditionals)
    {
    }

    protected override StringContextlessSchema CreateInstance() => new();

    protected override StringContextlessSchema CreateInstance(
        ContextlessRuleEngine<string> rules,
        bool allowNull,
        IReadOnlyList<(Func<string, bool>, ISchema<string>)>? conditionals)
        => new(rules, allowNull, conditionals);

    /// <summary>
    /// Creates a context-aware string schema with all rules from this schema.
    /// </summary>
    public StringContextSchema<TContext> Using<TContext>()
    {
        var schema = new StringContextSchema<TContext>(Rules.ToContext<TContext>());
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware string schema with a factory delegate for creating context data.
    /// </summary>
    public StringContextSchema<TContext> Using<TContext>(
        Func<string, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }

    /// <summary>
    /// Creates a context-aware string schema with a synchronous factory delegate for creating context data.
    /// </summary>
    public StringContextSchema<TContext> Using<TContext>(
        Func<string, IServiceProvider, TContext> factory)
    {
        return Using<TContext>().WithContextFactory((arg1, provider, _) => new ValueTask<TContext>(factory(arg1, provider)));
    }
}
