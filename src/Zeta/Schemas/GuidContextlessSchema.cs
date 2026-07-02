using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating Guid values.
/// </summary>
public sealed class GuidContextlessSchema : ContextlessSchema<Guid, GuidContextlessSchema>, IValueSchema<Guid, GuidContextlessSchema>
{
    internal GuidContextlessSchema()
    {
    }

    private GuidContextlessSchema(
        ContextlessRuleEngine<Guid> rules,
        bool allowNull,
        IReadOnlyList<(Func<Guid, bool>, ISchema<Guid>)>? conditionals)
        : base(rules, allowNull, conditionals)
    {
    }

    protected override GuidContextlessSchema CreateInstance() => new();

    protected override GuidContextlessSchema CreateInstance(
        ContextlessRuleEngine<Guid> rules,
        bool allowNull,
        IReadOnlyList<(Func<Guid, bool>, ISchema<Guid>)>? conditionals)
        => new(rules, allowNull, conditionals);

    /// <summary>
    /// Creates a context-aware Guid schema with all rules from this schema.
    /// </summary>
    public GuidContextSchema<TContext> Using<TContext>()
    {
        var schema = new GuidContextSchema<TContext>(Rules.ToContext<TContext>());
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware Guid schema with a factory delegate for creating context data.
    /// </summary>
    public GuidContextSchema<TContext> Using<TContext>(
        Func<Guid, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }

    /// <summary>
    /// Creates a context-aware Guid schema with a synchronous factory delegate for creating context data.
    /// </summary>
    public GuidContextSchema<TContext> Using<TContext>(
        Func<Guid, IServiceProvider, TContext> factory)
    {
        return Using<TContext>().WithContextFactory((arg1, provider, _) => new ValueTask<TContext>(factory(arg1, provider)));
    }
}
