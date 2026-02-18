using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating DateTime values.
/// </summary>
public sealed class DateTimeContextlessSchema : ContextlessSchema<DateTime, DateTimeContextlessSchema>
{
    internal DateTimeContextlessSchema()
    {
    }

    private DateTimeContextlessSchema(
        ContextlessRuleEngine<DateTime> rules,
        bool allowNull,
        IReadOnlyList<(Func<DateTime, bool>, ISchema<DateTime>)>? conditionals)
        : base(rules, allowNull, conditionals)
    {
    }

    protected override DateTimeContextlessSchema CreateInstance() => new();

    protected override DateTimeContextlessSchema CreateInstance(
        ContextlessRuleEngine<DateTime> rules,
        bool allowNull,
        IReadOnlyList<(Func<DateTime, bool>, ISchema<DateTime>)>? conditionals)
        => new(rules, allowNull, conditionals);

    public DateTimeContextlessSchema Min(DateTime min, string? message = null)
        => Append(new RefinementRule<DateTime>((val, exec) =>
            val >= min
                ? null
                : new ValidationError(exec.Path, "min_date", message ?? $"Must be at or after {min:O}")));

    public DateTimeContextlessSchema Max(DateTime max, string? message = null)
        => Append(new RefinementRule<DateTime>((val, exec) =>
            val <= max
                ? null
                : new ValidationError(exec.Path, "max_date", message ?? $"Must be at or before {max:O}")));

    public DateTimeContextlessSchema Past(string? message = null)
        => Append(new RefinementRule<DateTime>((val, exec) =>
            val < exec.TimeProvider.GetUtcNow().UtcDateTime
                ? null
                : new ValidationError(exec.Path, "past", message ?? "Must be in the past")));

    public DateTimeContextlessSchema Future(string? message = null)
        => Append(new RefinementRule<DateTime>((val, exec) =>
            val > exec.TimeProvider.GetUtcNow().UtcDateTime
                ? null
                : new ValidationError(exec.Path, "future", message ?? "Must be in the future")));

    public DateTimeContextlessSchema Between(DateTime min, DateTime max, string? message = null)
        => Append(new RefinementRule<DateTime>((val, exec) =>
            val >= min && val <= max
                ? null
                : new ValidationError(exec.Path, "between", message ?? $"Must be between {min:O} and {max:O}")));

    public DateTimeContextlessSchema Weekday(string? message = null)
        => Append(new RefinementRule<DateTime>((val, exec) =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday
                ? null
                : new ValidationError(exec.Path, "weekday", message ?? "Must be a weekday")));

    public DateTimeContextlessSchema Weekend(string? message = null)
        => Append(new RefinementRule<DateTime>((val, exec) =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday
                ? null
                : new ValidationError(exec.Path, "weekend", message ?? "Must be a weekend")));

    public DateTimeContextlessSchema WithinDays(int days, string? message = null)
        => Append(new RefinementRule<DateTime>((val, exec) =>
        {
            var now = exec.TimeProvider.GetUtcNow().UtcDateTime;
            var diff = Math.Abs((val - now).TotalDays);
            return diff <= days
                ? null
                : new ValidationError(exec.Path, "within_days", message ?? $"Must be within {days} days from now");
        }));

    public DateTimeContextlessSchema MinAge(int years, string? message = null)
        => Append(new RefinementRule<DateTime>((val, exec) =>
        {
            var today = exec.TimeProvider.GetUtcNow().UtcDateTime.Date;
            var age = today.Year - val.Year;
            if (val.Date > today.AddYears(-age)) age--;

            return age >= years
                ? null
                : new ValidationError(exec.Path, "min_age", message ?? $"Must be at least {years} years old");
        }));

    public DateTimeContextlessSchema MaxAge(int years, string? message = null)
        => Append(new RefinementRule<DateTime>((val, exec) =>
        {
            var today = exec.TimeProvider.GetUtcNow().UtcDateTime.Date;
            var age = today.Year - val.Year;
            if (val.Date > today.AddYears(-age)) age--;

            return age <= years
                ? null
                : new ValidationError(exec.Path, "max_age", message ?? $"Must be at most {years} years old");
        }));

    /// <summary>
    /// Creates a context-aware DateTime schema with all rules from this schema.
    /// </summary>
    public DateTimeContextSchema<TContext> Using<TContext>()
    {
        var schema = new DateTimeContextSchema<TContext>(Rules.ToContext<TContext>());
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware DateTime schema with a factory delegate for creating context data.
    /// </summary>
    public DateTimeContextSchema<TContext> Using<TContext>(
        Func<DateTime, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }
}
