using Zeta.Core;

namespace Zeta.Rules.Numeric;

/// <summary>
/// Validates that an int value is less than or equal to a maximum.
/// </summary>
public readonly struct MaxIntRule : IValidationRule<int>
{
    private readonly int _max;
    private readonly string? _message;

    public MaxIntRule(int max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(int value, ValidationRun context)
    {
        var error = value <= _max
            ? null
            : new ValidationError(context.PathSegments, "max_value", _message ?? $"Must be at most {_max}");
        return ValueTask.FromResult(error);
    }
}
/// <summary>
/// Validates that a double value is less than or equal to a maximum.
/// </summary>
public readonly struct MaxDoubleRule : IValidationRule<double>
{
    private readonly double _max;
    private readonly string? _message;

    public MaxDoubleRule(double max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(double value, ValidationRun context)
    {
        var error = value <= _max
            ? null
            : new ValidationError(context.PathSegments, "max_value", _message ?? $"Must be at most {_max}");
        return ValueTask.FromResult(error);
    }
}
/// <summary>
/// Validates that a decimal value is less than or equal to a maximum.
/// </summary>
public readonly struct MaxDecimalRule : IValidationRule<decimal>
{
    private readonly decimal _max;
    private readonly string? _message;

    public MaxDecimalRule(decimal max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(decimal value, ValidationRun context)
    {
        var error = value <= _max
            ? null
            : new ValidationError(context.PathSegments, "max_value", _message ?? $"Must be at most {_max}");
        return ValueTask.FromResult(error);
    }
}
