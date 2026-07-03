using Zeta.Core;

namespace Zeta.Rules.Numeric;

/// <summary>
/// Validates that an int value is positive (greater than 0).
/// </summary>
public readonly struct PositiveIntRule : IValidationRule<int>
{
    private readonly string? _message;

    public PositiveIntRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(int value, ValidationContext context)
    {
        var error = value > 0
            ? null
            : new ValidationError(context.PathSegments, "positive", _message ?? "Must be positive");
        return ValueTask.FromResult(error);
    }
}
/// <summary>
/// Validates that a double value is positive (greater than 0).
/// </summary>
public readonly struct PositiveDoubleRule : IValidationRule<double>
{
    private readonly string? _message;

    public PositiveDoubleRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(double value, ValidationContext context)
    {
        var error = value > 0
            ? null
            : new ValidationError(context.PathSegments, "positive", _message ?? "Must be positive");
        return ValueTask.FromResult(error);
    }
}
/// <summary>
/// Validates that a decimal value is positive (greater than 0).
/// </summary>
public readonly struct PositiveDecimalRule : IValidationRule<decimal>
{
    private readonly string? _message;

    public PositiveDecimalRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(decimal value, ValidationContext context)
    {
        var error = value > 0
            ? null
            : new ValidationError(context.PathSegments, "positive", _message ?? "Must be positive");
        return ValueTask.FromResult(error);
    }
}
