using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating DateTime values.
/// </summary>
public sealed class DateTimeContextlessSchema : ContextlessSchema<DateTime, DateTimeContextlessSchema>, IValueSchema<DateTime, DateTimeContextlessSchema>
{
    internal DateTimeContextlessSchema()
    {
    }

    private DateTimeContextlessSchema(
        ContextlessRuleEngine<DateTime> rules,
        bool allowNull,
        IReadOnlyList<(Func<DateTime, bool>, ISchema<DateTime>)>? conditionals)
        : base(rules, allowNull, conditionals)
    {
    }

    protected override DateTimeContextlessSchema CreateInstance() => new();

    protected override DateTimeContextlessSchema CreateInstance(
        ContextlessRuleEngine<DateTime> rules,
        bool allowNull,
        IReadOnlyList<(Func<DateTime, bool>, ISchema<DateTime>)>? conditionals)
        => new(rules, allowNull, conditionals);

    /// <summary>
    /// Creates a context-aware DateTime schema with all rules from this schema.
    /// </summary>
    public DateTimeContextSchema<TContext> Using<TContext>()
    {
        var schema = new DateTimeContextSchema<TContext>(Rules.ToContext<TContext>());
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware DateTime schema with a factory delegate for creating context data.
    /// </summary>
    public DateTimeContextSchema<TContext> Using<TContext>(
        Func<DateTime, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }

    /// <summary>
    /// Creates a context-aware DateTime schema with a synchronous factory delegate for creating context data.
    /// </summary>
    public DateTimeContextSchema<TContext> Using<TContext>(
        Func<DateTime, IServiceProvider, TContext> factory)
    {
        return Using<TContext>().WithContextFactory((arg1, provider, _) => new ValueTask<TContext>(factory(arg1, provider)));
    }
}
