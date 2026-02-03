using Zeta.Core;

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
        var error = value > 0
            ? null
            : new ValidationError(context.Path, "positive", _message ?? "Must be positive");
        return ValueTaskHelper.FromResult(error);
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
        var error = value > 0
            ? null
            : new ValidationError(context.Path, "positive", _message ?? "Must be positive");
        return ValueTaskHelper.FromResult(error);
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
            : new ValidationError(context.Path, "positive", _message ?? "Must be positive");
        return ValueTaskHelper.FromResult(error);
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
        var error = value > 0
            ? null
            : new ValidationError(context.Path, "positive", _message ?? "Must be positive");
        return ValueTaskHelper.FromResult(error);
    }
}
