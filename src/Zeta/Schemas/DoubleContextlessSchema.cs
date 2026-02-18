using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Numeric;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating double values.
/// </summary>
public sealed class DoubleContextlessSchema : ContextlessSchema<double, DoubleContextlessSchema>
{
    internal DoubleContextlessSchema()
    {
    }

    private DoubleContextlessSchema(
        ContextlessRuleEngine<double> rules,
        bool allowNull,
        IReadOnlyList<(Func<double, bool>, ISchema<double>)>? conditionals)
        : base(rules, allowNull, conditionals)
    {
    }

    protected override DoubleContextlessSchema CreateInstance() => new();

    protected override DoubleContextlessSchema CreateInstance(
        ContextlessRuleEngine<double> rules,
        bool allowNull,
        IReadOnlyList<(Func<double, bool>, ISchema<double>)>? conditionals)
        => new(rules, allowNull, conditionals);

    public DoubleContextlessSchema Min(double min, string? message = null)
        => Append(new MinDoubleRule(min, message));

    public DoubleContextlessSchema Max(double max, string? message = null)
        => Append(new MaxDoubleRule(max, message));

    public DoubleContextlessSchema Positive(string? message = null)
        => Append(new PositiveDoubleRule(message));

    public DoubleContextlessSchema Negative(string? message = null)
        => Append(new NegativeDoubleRule(message));

    public DoubleContextlessSchema Finite(string? message = null)
        => Append(new FiniteRule(message));

    /// <summary>
    /// Creates a context-aware double schema with all rules from this schema.
    /// </summary>
    public DoubleContextSchema<TContext> Using<TContext>()
    {
        var schema = new DoubleContextSchema<TContext>(Rules.ToContext<TContext>());
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware double schema with a factory delegate for creating context data.
    /// </summary>
    public DoubleContextSchema<TContext> Using<TContext>(
        Func<double, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }
}
