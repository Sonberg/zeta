using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating DateOnly values.
/// </summary>
public sealed class DateOnlyContextlessSchema : ContextlessSchema<DateOnly, DateOnlyContextlessSchema>, IValueSchema<DateOnly, DateOnlyContextlessSchema>
{
    internal DateOnlyContextlessSchema() { }

    private DateOnlyContextlessSchema(
        ContextlessRuleEngine<DateOnly> rules,
        bool allowNull,
        IReadOnlyList<(Func<DateOnly, bool>, ISchema<DateOnly>)>? conditionals)
        : base(rules, allowNull, conditionals)
    {
    }

    protected override DateOnlyContextlessSchema CreateInstance() => new();

    protected override DateOnlyContextlessSchema CreateInstance(
        ContextlessRuleEngine<DateOnly> rules,
        bool allowNull,
        IReadOnlyList<(Func<DateOnly, bool>, ISchema<DateOnly>)>? conditionals)
        => new(rules, allowNull, conditionals);

    /// <summary>
    /// Creates a context-aware DateOnly schema with all rules from this schema.
    /// </summary>
    public DateOnlyContextSchema<TContext> Using<TContext>()
    {
        var schema = new DateOnlyContextSchema<TContext>(Rules.ToContext<TContext>());
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware DateOnly schema with a factory delegate for creating context data.
    /// </summary>
    public DateOnlyContextSchema<TContext> Using<TContext>(
        Func<DateOnly, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }

    /// <summary>
    /// Creates a context-aware DateOnly schema with a synchronous factory delegate for creating context data.
    /// </summary>
    public DateOnlyContextSchema<TContext> Using<TContext>(
        Func<DateOnly, IServiceProvider, TContext> factory)
    {
        return Using<TContext>().WithContextFactory((arg1, provider, _) => new ValueTask<TContext>(factory(arg1, provider)));
    }
}
