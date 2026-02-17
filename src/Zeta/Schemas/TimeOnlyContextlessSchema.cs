#if !NETSTANDARD2_0
using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating TimeOnly values.
/// </summary>
public sealed class TimeOnlyContextlessSchema : ContextlessSchema<TimeOnly, TimeOnlyContextlessSchema>
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

    public TimeOnlyContextlessSchema Min(TimeOnly min, string? message = null)
        => Append(new RefinementRule<TimeOnly>((val, exec) =>
            val >= min
                ? null
                : new ValidationError(exec.Path, "min_time", message ?? $"Must be at or after {min:t}")));

    public TimeOnlyContextlessSchema Max(TimeOnly max, string? message = null)
        => Append(new RefinementRule<TimeOnly>((val, exec) =>
            val <= max
                ? null
                : new ValidationError(exec.Path, "max_time", message ?? $"Must be at or before {max:t}")));

    public TimeOnlyContextlessSchema Between(TimeOnly min, TimeOnly max, string? message = null)
        => Append(new RefinementRule<TimeOnly>((val, exec) =>
            val >= min && val <= max
                ? null
                : new ValidationError(exec.Path, "between", message ?? $"Must be between {min:t} and {max:t}")));

    public TimeOnlyContextlessSchema BusinessHours(TimeOnly? start = null, TimeOnly? end = null, string? message = null)
    {
        var businessStart = start ?? new TimeOnly(9, 0);
        var businessEnd = end ?? new TimeOnly(17, 0);
        return Append(new RefinementRule<TimeOnly>((val, exec) =>
            val >= businessStart && val <= businessEnd
                ? null
                : new ValidationError(exec.Path, "business_hours", message ?? $"Must be during business hours ({businessStart:t} - {businessEnd:t})")));
    }

    public TimeOnlyContextlessSchema Morning(string? message = null)
        => Append(new RefinementRule<TimeOnly>((val, exec) =>
            val.Hour < 12
                ? null
                : new ValidationError(exec.Path, "morning", message ?? "Must be in the morning (before 12:00)")));

    public TimeOnlyContextlessSchema Afternoon(string? message = null)
        => Append(new RefinementRule<TimeOnly>((val, exec) =>
            val.Hour >= 12 && val.Hour < 18
                ? null
                : new ValidationError(exec.Path, "afternoon", message ?? "Must be in the afternoon (12:00 - 18:00)")));

    public TimeOnlyContextlessSchema Evening(string? message = null)
        => Append(new RefinementRule<TimeOnly>((val, exec) =>
            val.Hour >= 18
                ? null
                : new ValidationError(exec.Path, "evening", message ?? "Must be in the evening (after 18:00)")));

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
}
#endif
