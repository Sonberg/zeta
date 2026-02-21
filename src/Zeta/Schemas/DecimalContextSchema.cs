using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Numeric;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating decimal values.
/// </summary>
public class DecimalContextSchema<TContext> : ContextSchema<decimal, TContext, DecimalContextSchema<TContext>>
{
    internal DecimalContextSchema() { }

    internal DecimalContextSchema(ContextRuleEngine<decimal, TContext> rules) : base(rules)
    {
    }

    private DecimalContextSchema(
        ContextRuleEngine<decimal, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<decimal, TContext>>? conditionals,
        Func<decimal, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
    }

    protected override DecimalContextSchema<TContext> CreateInstance() => new();

    private protected override DecimalContextSchema<TContext> CreateInstance(
        ContextRuleEngine<decimal, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<decimal, TContext>>? conditionals,
        Func<decimal, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, allowNull, conditionals, contextFactory);

    public DecimalContextSchema<TContext> Min(decimal min, string? message = null)
        => Append(new MinDecimalRule<TContext>(min, message));

    public DecimalContextSchema<TContext> Max(decimal max, string? message = null)
        => Append(new MaxDecimalRule<TContext>(max, message));

    public DecimalContextSchema<TContext> Range(decimal min, decimal max, string? message = null)
    {
        if (min > max)
            throw new ArgumentOutOfRangeException(nameof(min), "min must be less than or equal to max.");

        return Min(min, message).Max(max, message);
    }

    public DecimalContextSchema<TContext> Positive(string? message = null)
        => Append(new PositiveDecimalRule<TContext>(message));

    public DecimalContextSchema<TContext> Negative(string? message = null)
        => Append(new NegativeDecimalRule<TContext>(message));

    public DecimalContextSchema<TContext> Precision(int maxDecimalPlaces, string? message = null)
        => Append(new PrecisionRule<TContext>(maxDecimalPlaces, message));

    public DecimalContextSchema<TContext> MultipleOf(decimal step, string? message = null)
        => Append(new MultipleOfRule<TContext>(step, message));
}
