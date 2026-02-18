using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Numeric;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating integer values.
/// </summary>
public sealed class IntContextlessSchema : ContextlessSchema<int, IntContextlessSchema>
{
    internal IntContextlessSchema()
    {
    }

    private IntContextlessSchema(
        ContextlessRuleEngine<int> rules,
        bool allowNull,
        IReadOnlyList<(Func<int, bool>, ISchema<int>)>? conditionals)
        : base(rules, allowNull, conditionals)
    {
    }

    protected override IntContextlessSchema CreateInstance() => new();

    protected override IntContextlessSchema CreateInstance(
        ContextlessRuleEngine<int> rules,
        bool allowNull,
        IReadOnlyList<(Func<int, bool>, ISchema<int>)>? conditionals)
        => new(rules, allowNull, conditionals);

    public IntContextlessSchema Min(int min, string? message = null)
        => Append(new MinIntRule(min, message));

    public IntContextlessSchema Max(int max, string? message = null)
        => Append(new MaxIntRule(max, message));

    /// <summary>
    /// Creates a context-aware int schema with all rules from this schema.
    /// </summary>
    public IntContextSchema<TContext> Using<TContext>()
    {
        var schema = new IntContextSchema<TContext>(Rules.ToContext<TContext>());
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware int schema with a factory delegate for creating context data.
    /// </summary>
    public IntContextSchema<TContext> Using<TContext>(
        Func<int, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }
}
