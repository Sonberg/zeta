using Zeta.Core;

namespace Zeta.Rules.Numeric;

/// <summary>
/// Validates that a double value is negative (less than 0).
/// </summary>
public readonly struct NegativeDoubleRule : IValidationRule<double>
{
    private readonly string? _message;

    public NegativeDoubleRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(double value, ValidationContext context)
    {
        var error = value < 0
            ? null
            : new ValidationError(context.Path, "negative", _message ?? "Must be negative");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a double value is negative (less than 0).
/// </summary>
public readonly struct NegativeDoubleRule<TContext> : IValidationRule<double, TContext>
{
    private readonly string? _message;

    public NegativeDoubleRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(double value, ValidationContext<TContext> context)
    {
        var error = value < 0
            ? null
            : new ValidationError(context.Path, "negative", _message ?? "Must be negative");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a decimal value is negative (less than 0).
/// </summary>
public readonly struct NegativeDecimalRule : IValidationRule<decimal>
{
    private readonly string? _message;

    public NegativeDecimalRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(decimal value, ValidationContext context)
    {
        var error = value < 0
            ? null
            : new ValidationError(context.Path, "negative", _message ?? "Must be negative");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a decimal value is negative (less than 0).
/// </summary>
public readonly struct NegativeDecimalRule<TContext> : IValidationRule<decimal, TContext>
{
    private readonly string? _message;

    public NegativeDecimalRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(decimal value, ValidationContext<TContext> context)
    {
        var error = value < 0
            ? null
            : new ValidationError(context.Path, "negative", _message ?? "Must be negative");
        return ValueTaskHelper.FromResult(error);
    }
}
