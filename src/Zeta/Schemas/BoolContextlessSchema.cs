using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating boolean values.
/// </summary>
public sealed class BoolContextlessSchema : ContextlessSchema<bool, BoolContextlessSchema>
{
    internal BoolContextlessSchema()
    {
    }

    private BoolContextlessSchema(
        ContextlessRuleEngine<bool> rules,
        bool allowNull,
        IReadOnlyList<(Func<bool, bool>, ISchema<bool>)>? conditionals)
        : base(rules, allowNull, conditionals)
    {
    }

    protected override BoolContextlessSchema CreateInstance() => new();

    protected override BoolContextlessSchema CreateInstance(
        ContextlessRuleEngine<bool> rules,
        bool allowNull,
        IReadOnlyList<(Func<bool, bool>, ISchema<bool>)>? conditionals)
        => new(rules, allowNull, conditionals);

    public BoolContextlessSchema IsTrue(string? message = null)
        => Append(new RefinementRule<bool>((val, exec) =>
            val
                ? null
                : new ValidationError(exec.Path, "is_true", message ?? "Must be true")));

    public BoolContextlessSchema IsFalse(string? message = null)
        => Append(new RefinementRule<bool>((val, exec) =>
            !val
                ? null
                : new ValidationError(exec.Path, "is_false", message ?? "Must be false")));

    /// <summary>
    /// Creates a context-aware bool schema with all rules from this schema.
    /// </summary>
    public BoolContextSchema<TContext> Using<TContext>()
    {
        var schema = new BoolContextSchema<TContext>(Rules.ToContext<TContext>());
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware bool schema with a factory delegate for creating context data.
    /// </summary>
    public BoolContextSchema<TContext> Using<TContext>(
        Func<bool, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }
}
