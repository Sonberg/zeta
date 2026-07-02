using Zeta.Core;

namespace Zeta.Rules;

/// <summary>
/// Validates that a TimeOnly value is at or after a minimum.
/// </summary>
public readonly struct TimeOnlyMinRule : IValidationRule<TimeOnly>
{
    private readonly TimeOnly _min;
    private readonly string? _message;

    public TimeOnlyMinRule(TimeOnly min, string? message = null)
    {
        _min = min;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(TimeOnly value, ValidationContext context)
    {
        var error = value >= _min
            ? null
            : new ValidationError(context.PathSegments, "min_time", _message ?? $"Must be at or after {_min:t}");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a TimeOnly value is at or before a maximum.
/// </summary>
public readonly struct TimeOnlyMaxRule : IValidationRule<TimeOnly>
{
    private readonly TimeOnly _max;
    private readonly string? _message;

    public TimeOnlyMaxRule(TimeOnly max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(TimeOnly value, ValidationContext context)
    {
        var error = value <= _max
            ? null
            : new ValidationError(context.PathSegments, "max_time", _message ?? $"Must be at or before {_max:t}");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a TimeOnly value falls within an inclusive range.
/// </summary>
public readonly struct TimeOnlyBetweenRule : IValidationRule<TimeOnly>
{
    private readonly TimeOnly _min;
    private readonly TimeOnly _max;
    private readonly string? _message;

    public TimeOnlyBetweenRule(TimeOnly min, TimeOnly max, string? message = null)
    {
        _min = min;
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(TimeOnly value, ValidationContext context)
    {
        var error = value >= _min && value <= _max
            ? null
            : new ValidationError(context.PathSegments, "between", _message ?? $"Must be between {_min:t} and {_max:t}");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a TimeOnly value falls within business hours.
/// </summary>
public readonly struct TimeOnlyBusinessHoursRule : IValidationRule<TimeOnly>
{
    private readonly TimeOnly _start;
    private readonly TimeOnly _end;
    private readonly string? _message;

    public TimeOnlyBusinessHoursRule(TimeOnly start, TimeOnly end, string? message = null)
    {
        _start = start;
        _end = end;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(TimeOnly value, ValidationContext context)
    {
        var error = value >= _start && value <= _end
            ? null
            : new ValidationError(context.PathSegments, "business_hours", _message ?? $"Must be during business hours ({_start:t} - {_end:t})");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a TimeOnly value falls in the morning (before 12:00).
/// </summary>
public readonly struct TimeOnlyMorningRule : IValidationRule<TimeOnly>
{
    private readonly string? _message;

    public TimeOnlyMorningRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(TimeOnly value, ValidationContext context)
    {
        var error = value.Hour < 12
            ? null
            : new ValidationError(context.PathSegments, "morning", _message ?? "Must be in the morning (before 12:00)");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a TimeOnly value falls in the afternoon (12:00 - 18:00).
/// </summary>
public readonly struct TimeOnlyAfternoonRule : IValidationRule<TimeOnly>
{
    private readonly string? _message;

    public TimeOnlyAfternoonRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(TimeOnly value, ValidationContext context)
    {
        var error = value.Hour >= 12 && value.Hour < 18
            ? null
            : new ValidationError(context.PathSegments, "afternoon", _message ?? "Must be in the afternoon (12:00 - 18:00)");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a TimeOnly value falls in the evening (after 18:00).
/// </summary>
public readonly struct TimeOnlyEveningRule : IValidationRule<TimeOnly>
{
    private readonly string? _message;

    public TimeOnlyEveningRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(TimeOnly value, ValidationContext context)
    {
        var error = value.Hour >= 18
            ? null
            : new ValidationError(context.PathSegments, "evening", _message ?? "Must be in the evening (after 18:00)");
        return ValueTaskHelper.FromResult(error);
    }
}
