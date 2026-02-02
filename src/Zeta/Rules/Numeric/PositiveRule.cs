using Zeta.Core;
using Zeta.Validation;

namespace Zeta.Rules.Numeric;

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
        return new ValueTask<ValidationError?>(
            NumericValidators.Positive(value, context.Path, _message));
    }
}

/// <summary>
/// Context-aware version: Validates that a double value is positive (greater than 0).
/// </summary>
public readonly struct PositiveDoubleRule<TContext> : IValidationRule<double, TContext>
{
    private readonly string? _message;

    public PositiveDoubleRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(double value, ValidationContext<TContext> context)
    {
        return new ValueTask<ValidationError?>(
            NumericValidators.Positive(value, context.Path, _message));
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
        return new ValueTask<ValidationError?>(
            NumericValidators.Positive(value, context.Path, _message));
    }
}

/// <summary>
/// Context-aware version: Validates that a decimal value is positive (greater than 0).
/// </summary>
public readonly struct PositiveDecimalRule<TContext> : IValidationRule<decimal, TContext>
{
    private readonly string? _message;

    public PositiveDecimalRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(decimal value, ValidationContext<TContext> context)
    {
        return new ValueTask<ValidationError?>(
            NumericValidators.Positive(value, context.Path, _message));
    }
}
