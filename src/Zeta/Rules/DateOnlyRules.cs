using Zeta.Core;

namespace Zeta.Rules;

/// <summary>
/// Validates that a DateOnly value is at or after a minimum.
/// </summary>
public readonly struct DateOnlyMinRule : IValidationRule<DateOnly>
{
    private readonly DateOnly _min;
    private readonly string? _message;

    public DateOnlyMinRule(DateOnly min, string? message = null)
    {
        _min = min;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateOnly value, ValidationContext context)
    {
        var error = value >= _min
            ? null
            : new ValidationError(context.PathSegments, "min_date", _message ?? $"Must be at or after {_min:O}");
        return ValueTask.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateOnly value is at or before a maximum.
/// </summary>
public readonly struct DateOnlyMaxRule : IValidationRule<DateOnly>
{
    private readonly DateOnly _max;
    private readonly string? _message;

    public DateOnlyMaxRule(DateOnly max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateOnly value, ValidationContext context)
    {
        var error = value <= _max
            ? null
            : new ValidationError(context.PathSegments, "max_date", _message ?? $"Must be at or before {_max:O}");
        return ValueTask.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateOnly value is in the past.
/// </summary>
public readonly struct DateOnlyPastRule : IValidationRule<DateOnly>
{
    private readonly string? _message;

    public DateOnlyPastRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateOnly value, ValidationContext context)
    {
        var today = DateOnly.FromDateTime(context.TimeProvider.GetUtcNow().UtcDateTime);
        var error = value < today
            ? null
            : new ValidationError(context.PathSegments, "past", _message ?? "Must be in the past");
        return ValueTask.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateOnly value is in the future.
/// </summary>
public readonly struct DateOnlyFutureRule : IValidationRule<DateOnly>
{
    private readonly string? _message;

    public DateOnlyFutureRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateOnly value, ValidationContext context)
    {
        var today = DateOnly.FromDateTime(context.TimeProvider.GetUtcNow().UtcDateTime);
        var error = value > today
            ? null
            : new ValidationError(context.PathSegments, "future", _message ?? "Must be in the future");
        return ValueTask.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateOnly value falls within an inclusive range.
/// </summary>
public readonly struct DateOnlyBetweenRule : IValidationRule<DateOnly>
{
    private readonly DateOnly _min;
    private readonly DateOnly _max;
    private readonly string? _message;

    public DateOnlyBetweenRule(DateOnly min, DateOnly max, string? message = null)
    {
        _min = min;
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateOnly value, ValidationContext context)
    {
        var error = value >= _min && value <= _max
            ? null
            : new ValidationError(context.PathSegments, "between", _message ?? $"Must be between {_min:O} and {_max:O}");
        return ValueTask.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateOnly value falls on a weekday.
/// </summary>
public readonly struct DateOnlyWeekdayRule : IValidationRule<DateOnly>
{
    private readonly string? _message;

    public DateOnlyWeekdayRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateOnly value, ValidationContext context)
    {
        var error = value.DayOfWeek != DayOfWeek.Saturday && value.DayOfWeek != DayOfWeek.Sunday
            ? null
            : new ValidationError(context.PathSegments, "weekday", _message ?? "Must be a weekday");
        return ValueTask.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateOnly value falls on a weekend.
/// </summary>
public readonly struct DateOnlyWeekendRule : IValidationRule<DateOnly>
{
    private readonly string? _message;

    public DateOnlyWeekendRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateOnly value, ValidationContext context)
    {
        var error = value.DayOfWeek == DayOfWeek.Saturday || value.DayOfWeek == DayOfWeek.Sunday
            ? null
            : new ValidationError(context.PathSegments, "weekend", _message ?? "Must be a weekend");
        return ValueTask.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateOnly value represents an age of at least a given number of years.
/// </summary>
public readonly struct DateOnlyMinAgeRule : IValidationRule<DateOnly>
{
    private readonly int _years;
    private readonly string? _message;

    public DateOnlyMinAgeRule(int years, string? message = null)
    {
        _years = years;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateOnly value, ValidationContext context)
    {
        var today = DateOnly.FromDateTime(context.TimeProvider.GetUtcNow().UtcDateTime);
        var age = today.Year - value.Year;
        if (value > today.AddYears(-age)) age--;

        var error = age >= _years
            ? null
            : new ValidationError(context.PathSegments, "min_age", _message ?? $"Must be at least {_years} years old");
        return ValueTask.FromResult(error);
    }
}

/// <summary>
/// Validates that a DateOnly value represents an age of at most a given number of years.
/// </summary>
public readonly struct DateOnlyMaxAgeRule : IValidationRule<DateOnly>
{
    private readonly int _years;
    private readonly string? _message;

    public DateOnlyMaxAgeRule(int years, string? message = null)
    {
        _years = years;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(DateOnly value, ValidationContext context)
    {
        var today = DateOnly.FromDateTime(context.TimeProvider.GetUtcNow().UtcDateTime);
        var age = today.Year - value.Year;
        if (value > today.AddYears(-age)) age--;

        var error = age <= _years
            ? null
            : new ValidationError(context.PathSegments, "max_age", _message ?? $"Must be at most {_years} years old");
        return ValueTask.FromResult(error);
    }
}
