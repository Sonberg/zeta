using Zeta.Core;
using Zeta.Validation;

namespace Zeta.Rules.Numeric;

/// <summary>
/// Validates that a double value is finite (not NaN or infinity).
/// </summary>
public readonly struct FiniteRule : IValidationRule<double>
{
    private readonly string? _message;

    public FiniteRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(double value, ValidationContext context)
    {
        return new ValueTask<ValidationError?>(
            NumericValidators.Finite(value, context.Path, _message));
    }
}

/// <summary>
/// Context-aware version: Validates that a double value is finite (not NaN or infinity).
/// </summary>
public readonly struct FiniteRule<TContext> : IValidationRule<double, TContext>
{
    private readonly string? _message;

    public FiniteRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(double value, ValidationContext<TContext> context)
    {
        return new ValueTask<ValidationError?>(
            NumericValidators.Finite(value, context.Path, _message));
    }
}
