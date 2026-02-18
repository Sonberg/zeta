#if !NETSTANDARD2_0
using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating DateOnly values.
/// </summary>
public sealed class DateOnlyContextlessSchema : ContextlessSchema<DateOnly, DateOnlyContextlessSchema>
{
    public DateOnlyContextlessSchema() { }

    private DateOnlyContextlessSchema(
        ContextlessRuleEngine<DateOnly> rules,
        bool allowNull,
        IReadOnlyList<(Func<DateOnly, bool>, ISchema<DateOnly>)>? conditionals)
        : base(rules, allowNull, conditionals)
    {
    }

    protected override DateOnlyContextlessSchema CreateInstance() => new();

    protected override DateOnlyContextlessSchema CreateInstance(
        ContextlessRuleEngine<DateOnly> rules,
        bool allowNull,
        IReadOnlyList<(Func<DateOnly, bool>, ISchema<DateOnly>)>? conditionals)
        => new(rules, allowNull, conditionals);

    public DateOnlyContextlessSchema Min(DateOnly min, string? message = null)
        => Append(new RefinementRule<DateOnly>((val, exec) =>
            val >= min
                ? null
                : new ValidationError(exec.Path, "min_date", message ?? $"Must be at or after {min:O}")));

    public DateOnlyContextlessSchema Max(DateOnly max, string? message = null)
        => Append(new RefinementRule<DateOnly>((val, exec) =>
            val <= max
                ? null
                : new ValidationError(exec.Path, "max_date", message ?? $"Must be at or before {max:O}")));

    public DateOnlyContextlessSchema Past(string? message = null)
        => Append(new RefinementRule<DateOnly>((val, exec) =>
        {
            var today = DateOnly.FromDateTime(exec.TimeProvider.GetUtcNow().UtcDateTime);
            return val < today
                ? null
                : new ValidationError(exec.Path, "past", message ?? "Must be in the past");
        }));

    public DateOnlyContextlessSchema Future(string? message = null)
        => Append(new RefinementRule<DateOnly>((val, exec) =>
        {
            var today = DateOnly.FromDateTime(exec.TimeProvider.GetUtcNow().UtcDateTime);
            return val > today
                ? null
                : new ValidationError(exec.Path, "future", message ?? "Must be in the future");
        }));

    public DateOnlyContextlessSchema Between(DateOnly min, DateOnly max, string? message = null)
        => Append(new RefinementRule<DateOnly>((val, exec) =>
            val >= min && val <= max
                ? null
                : new ValidationError(exec.Path, "between", message ?? $"Must be between {min:O} and {max:O}")));

    public DateOnlyContextlessSchema Weekday(string? message = null)
        => Append(new RefinementRule<DateOnly>((val, exec) =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday
                ? null
                : new ValidationError(exec.Path, "weekday", message ?? "Must be a weekday")));

    public DateOnlyContextlessSchema Weekend(string? message = null)
        => Append(new RefinementRule<DateOnly>((val, exec) =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday
                ? null
                : new ValidationError(exec.Path, "weekend", message ?? "Must be a weekend")));

    public DateOnlyContextlessSchema MinAge(int years, string? message = null)
        => Append(new RefinementRule<DateOnly>((val, exec) =>
        {
            var today = DateOnly.FromDateTime(exec.TimeProvider.GetUtcNow().UtcDateTime);
            var age = today.Year - val.Year;
            if (val > today.AddYears(-age)) age--;

            return age >= years
                ? null
                : new ValidationError(exec.Path, "min_age", message ?? $"Must be at least {years} years old");
        }));

    public DateOnlyContextlessSchema MaxAge(int years, string? message = null)
        => Append(new RefinementRule<DateOnly>((val, exec) =>
        {
            var today = DateOnly.FromDateTime(exec.TimeProvider.GetUtcNow().UtcDateTime);
            var age = today.Year - val.Year;
            if (val > today.AddYears(-age)) age--;

            return age <= years
                ? null
                : new ValidationError(exec.Path, "max_age", message ?? $"Must be at most {years} years old");
        }));

    /// <summary>
    /// Creates a context-aware DateOnly schema with all rules from this schema.
    /// </summary>
    public DateOnlyContextSchema<TContext> Using<TContext>()
    {
        var schema = new DateOnlyContextSchema<TContext>(Rules.ToContext<TContext>());
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware DateOnly schema with a factory delegate for creating context data.
    /// </summary>
    public DateOnlyContextSchema<TContext> Using<TContext>(
        Func<DateOnly, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }
}
#endif
