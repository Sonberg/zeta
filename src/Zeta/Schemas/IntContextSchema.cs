using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Numeric;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating integer values.
/// </summary>
public class IntContextSchema<TContext> : ContextSchema<int, TContext, IntContextSchema<TContext>>
{
    internal IntContextSchema() { }

    internal IntContextSchema(ContextRuleEngine<int, TContext> rules) : base(rules) { }

    private IntContextSchema(
        ContextRuleEngine<int, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<int, TContext>>? conditionals,
        Func<int, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
    }

    protected override IntContextSchema<TContext> CreateInstance() => new();

    private protected override IntContextSchema<TContext> CreateInstance(
        ContextRuleEngine<int, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<int, TContext>>? conditionals,
        Func<int, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, allowNull, conditionals, contextFactory);

    public IntContextSchema<TContext> Min(int min, string? message = null)
        => Append(new MinIntRule<TContext>(min, message));

    public IntContextSchema<TContext> Max(int max, string? message = null)
        => Append(new MaxIntRule<TContext>(max, message));
}
