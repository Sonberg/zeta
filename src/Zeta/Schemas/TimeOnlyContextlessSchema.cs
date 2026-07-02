#if !NETSTANDARD2_0
using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating TimeOnly values.
/// </summary>
public sealed class TimeOnlyContextlessSchema : ContextlessSchema<TimeOnly, TimeOnlyContextlessSchema>, IValueSchema<TimeOnly, TimeOnlyContextlessSchema>
{
    internal TimeOnlyContextlessSchema() { }

    private TimeOnlyContextlessSchema(
        ContextlessRuleEngine<TimeOnly> rules,
        bool allowNull,
        IReadOnlyList<(Func<TimeOnly, bool>, ISchema<TimeOnly>)>? conditionals)
        : base(rules, allowNull, conditionals)
    {
    }

    protected override TimeOnlyContextlessSchema CreateInstance() => new();

    protected override TimeOnlyContextlessSchema CreateInstance(
        ContextlessRuleEngine<TimeOnly> rules,
        bool allowNull,
        IReadOnlyList<(Func<TimeOnly, bool>, ISchema<TimeOnly>)>? conditionals)
        => new(rules, allowNull, conditionals);

    /// <summary>
    /// Creates a context-aware TimeOnly schema with all rules from this schema.
    /// </summary>
    public TimeOnlyContextSchema<TContext> Using<TContext>()
    {
        var schema = new TimeOnlyContextSchema<TContext>(Rules.ToContext<TContext>());
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware TimeOnly schema with a factory delegate for creating context data.
    /// </summary>
    public TimeOnlyContextSchema<TContext> Using<TContext>(
        Func<TimeOnly, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }

    /// <summary>
    /// Creates a context-aware TimeOnly schema with a synchronous factory delegate for creating context data.
    /// </summary>
    public TimeOnlyContextSchema<TContext> Using<TContext>(
        Func<TimeOnly, IServiceProvider, TContext> factory)
    {
        return Using<TContext>().WithContextFactory((arg1, provider, _) => new ValueTask<TContext>(factory(arg1, provider)));
    }
}
#endif
