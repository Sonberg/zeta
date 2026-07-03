using Zeta.Core;

namespace Zeta.Rules.Numeric;

/// <summary>
/// Validates that an int value is a multiple of a specified number.
/// </summary>
public readonly struct MultipleOfIntRule : IValidationRule<int>
{
    private readonly int _divisor;
    private readonly string? _message;

    public MultipleOfIntRule(int divisor, string? message = null)
    {
        _divisor = divisor;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(int value, ValidationContext context)
    {
        var error = value % _divisor == 0
            ? null
            : new ValidationError(context.PathSegments, "multiple_of", _message ?? $"Must be a multiple of {_divisor}");
        return ValueTask.FromResult(error);
    }
}

/// <summary>
/// Validates that a double value is a multiple of a specified number.
/// </summary>
public readonly struct MultipleOfDoubleRule : IValidationRule<double>
{
    private readonly double _divisor;
    private readonly string? _message;

    public MultipleOfDoubleRule(double divisor, string? message = null)
    {
        _divisor = divisor;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(double value, ValidationContext context)
    {
        var error = value % _divisor == 0
            ? null
            : new ValidationError(context.PathSegments, "multiple_of", _message ?? $"Must be a multiple of {_divisor}");
        return ValueTask.FromResult(error);
    }
}

/// <summary>
/// Validates that a decimal value is a multiple of a specified number.
/// </summary>
public readonly struct MultipleOfRule : IValidationRule<decimal>
{
    private readonly decimal _divisor;
    private readonly string? _message;

    public MultipleOfRule(decimal divisor, string? message = null)
    {
        _divisor = divisor;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(decimal value, ValidationContext context)
    {
        var error = value % _divisor == 0
            ? null
            : new ValidationError(context.PathSegments, "multiple_of", _message ?? $"Must be a multiple of {_divisor}");
        return ValueTask.FromResult(error);
    }
}
