using Zeta.Core;

namespace Zeta.Rules;

/// <summary>
/// Validates that a DateTime value is at or after a minimum.
/// </summary>
public readonly struct DateTimeMinRule : IValidationRule<DateTime>
{
    private readonly DateTime _min;
    private readonly string? _message;

    public DateTimeMinRule(DateTime min, string? message = null)
    {
        _min = min;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateTime value, ValidationContext context)
    {
        var error = value >= _min
            ? null
            : new ValidationError(context.PathSegments, "min_date", _message ?? $"Must be at or after {_min:O}");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateTime value is at or before a maximum.
/// </summary>
public readonly struct DateTimeMaxRule : IValidationRule<DateTime>
{
    private readonly DateTime _max;
    private readonly string? _message;

    public DateTimeMaxRule(DateTime max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateTime value, ValidationContext context)
    {
        var error = value <= _max
            ? null
            : new ValidationError(context.PathSegments, "max_date", _message ?? $"Must be at or before {_max:O}");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateTime value is in the past.
/// </summary>
public readonly struct DateTimePastRule : IValidationRule<DateTime>
{
    private readonly string? _message;

    public DateTimePastRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateTime value, ValidationContext context)
    {
        var error = value < context.TimeProvider.GetUtcNow().UtcDateTime
            ? null
            : new ValidationError(context.PathSegments, "past", _message ?? "Must be in the past");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateTime value is in the future.
/// </summary>
public readonly struct DateTimeFutureRule : IValidationRule<DateTime>
{
    private readonly string? _message;

    public DateTimeFutureRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateTime value, ValidationContext context)
    {
        var error = value > context.TimeProvider.GetUtcNow().UtcDateTime
            ? null
            : new ValidationError(context.PathSegments, "future", _message ?? "Must be in the future");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateTime value falls within an inclusive range.
/// </summary>
public readonly struct DateTimeBetweenRule : IValidationRule<DateTime>
{
    private readonly DateTime _min;
    private readonly DateTime _max;
    private readonly string? _message;

    public DateTimeBetweenRule(DateTime min, DateTime max, string? message = null)
    {
        _min = min;
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateTime value, ValidationContext context)
    {
        var error = value >= _min && value <= _max
            ? null
            : new ValidationError(context.PathSegments, "between", _message ?? $"Must be between {_min:O} and {_max:O}");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateTime value falls on a weekday.
/// </summary>
public readonly struct DateTimeWeekdayRule : IValidationRule<DateTime>
{
    private readonly string? _message;

    public DateTimeWeekdayRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateTime value, ValidationContext context)
    {
        var error = value.DayOfWeek != DayOfWeek.Saturday && value.DayOfWeek != DayOfWeek.Sunday
            ? null
            : new ValidationError(context.PathSegments, "weekday", _message ?? "Must be a weekday");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateTime value falls on a weekend.
/// </summary>
public readonly struct DateTimeWeekendRule : IValidationRule<DateTime>
{
    private readonly string? _message;

    public DateTimeWeekendRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateTime value, ValidationContext context)
    {
        var error = value.DayOfWeek == DayOfWeek.Saturday || value.DayOfWeek == DayOfWeek.Sunday
            ? null
            : new ValidationError(context.PathSegments, "weekend", _message ?? "Must be a weekend");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateTime value is within a given number of days from now.
/// </summary>
public readonly struct DateTimeWithinDaysRule : IValidationRule<DateTime>
{
    private readonly int _days;
    private readonly string? _message;

    public DateTimeWithinDaysRule(int days, string? message = null)
    {
        _days = days;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateTime value, ValidationContext context)
    {
        var now = context.TimeProvider.GetUtcNow().UtcDateTime;
        var diff = Math.Abs((value - now).TotalDays);
        var error = diff <= _days
            ? null
            : new ValidationError(context.PathSegments, "within_days", _message ?? $"Must be within {_days} days from now");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateTime value represents an age of at least a given number of years.
/// </summary>
public readonly struct DateTimeMinAgeRule : IValidationRule<DateTime>
{
    private readonly int _years;
    private readonly string? _message;

    public DateTimeMinAgeRule(int years, string? message = null)
    {
        _years = years;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateTime value, ValidationContext context)
    {
        var today = context.TimeProvider.GetUtcNow().UtcDateTime.Date;
        var age = today.Year - value.Year;
        if (value.Date > today.AddYears(-age)) age--;

        var error = age >= _years
            ? null
            : new ValidationError(context.PathSegments, "min_age", _message ?? $"Must be at least {_years} years old");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateTime value represents an age of at most a given number of years.
/// </summary>
public readonly struct DateTimeMaxAgeRule : IValidationRule<DateTime>
{
    private readonly int _years;
    private readonly string? _message;

    public DateTimeMaxAgeRule(int years, string? message = null)
    {
        _years = years;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateTime value, ValidationContext context)
    {
        var today = context.TimeProvider.GetUtcNow().UtcDateTime.Date;
        var age = today.Year - value.Year;
        if (value.Date > today.AddYears(-age)) age--;

        var error = age <= _years
            ? null
            : new ValidationError(context.PathSegments, "max_age", _message ?? $"Must be at most {_years} years old");
        return ValueTaskHelper.FromResult(error);
    }
}
