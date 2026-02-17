using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Numeric;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating decimal values.
/// </summary>
public sealed class DecimalContextlessSchema : ContextlessSchema<decimal, DecimalContextlessSchema>
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

    public DecimalContextlessSchema Min(decimal min, string? message = null)
        => Append(new MinDecimalRule(min, message));

    public DecimalContextlessSchema Max(decimal max, string? message = null)
        => Append(new MaxDecimalRule(max, message));

    public DecimalContextlessSchema Positive(string? message = null)
        => Append(new PositiveDecimalRule(message));

    public DecimalContextlessSchema Negative(string? message = null)
        => Append(new NegativeDecimalRule(message));

    public DecimalContextlessSchema Precision(int maxDecimalPlaces, string? message = null)
        => Append(new PrecisionRule(maxDecimalPlaces, message));

    public DecimalContextlessSchema MultipleOf(decimal step, string? message = null)
        => Append(new MultipleOfRule(step, message));

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
