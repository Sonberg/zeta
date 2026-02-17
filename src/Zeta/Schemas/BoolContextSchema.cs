using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating boolean values.
/// </summary>
public class BoolContextSchema<TContext> : ContextSchema<bool, TContext, BoolContextSchema<TContext>>
{
    internal BoolContextSchema() { }

    internal BoolContextSchema(ContextRuleEngine<bool, TContext> rules) : base(rules) { }

    private BoolContextSchema(
        ContextRuleEngine<bool, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<bool, TContext>>? conditionals,
        Func<bool, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
    }

    protected override BoolContextSchema<TContext> CreateInstance() => new();

    protected override BoolContextSchema<TContext> CreateInstance(
        ContextRuleEngine<bool, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<bool, TContext>>? conditionals,
        Func<bool, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, allowNull, conditionals, contextFactory);

    public BoolContextSchema<TContext> IsTrue(string? message = null)
        => Append(new RefinementRule<bool, TContext>((val, ctx) =>
            val
                ? null
                : new ValidationError(ctx.Path, "is_true", message ?? "Must be true")));

    public BoolContextSchema<TContext> IsFalse(string? message = null)
        => Append(new RefinementRule<bool, TContext>((val, ctx) =>
            !val
                ? null
                : new ValidationError(ctx.Path, "is_false", message ?? "Must be false")));
}
