using Zeta.Core;
using Zeta.Validation;

namespace Zeta.Rules.Numeric;

/// <summary>
/// Validates that a decimal value has at most the specified number of decimal places.
/// </summary>
public readonly struct PrecisionRule : IValidationRule<decimal>
{
    private readonly int _decimals;
    private readonly string? _message;

    public PrecisionRule(int decimals, string? message = null)
    {
        _decimals = decimals;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(decimal value, ValidationContext context)
    {
        return new ValueTask<ValidationError?>(
            NumericValidators.Precision(value, _decimals, context.Path, _message));
    }
}

/// <summary>
/// Context-aware version: Validates that a decimal value has at most the specified number of decimal places.
/// </summary>
public readonly struct PrecisionRule<TContext> : IValidationRule<decimal, TContext>
{
    private readonly int _decimals;
    private readonly string? _message;

    public PrecisionRule(int decimals, string? message = null)
    {
        _decimals = decimals;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(decimal value, ValidationContext<TContext> context)
    {
        return new ValueTask<ValidationError?>(
            NumericValidators.Precision(value, _decimals, context.Path, _message));
    }
}
