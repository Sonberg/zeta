using Zeta.Core;
using Zeta.Validation;

namespace Zeta.Rules.Numeric;

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
        return new ValueTask<ValidationError?>(
            NumericValidators.MultipleOf(value, _divisor, context.Path, _message));
    }
}

/// <summary>
/// Context-aware version: Validates that a decimal value is a multiple of a specified number.
/// </summary>
public readonly struct MultipleOfRule<TContext> : IValidationRule<decimal, TContext>
{
    private readonly decimal _divisor;
    private readonly string? _message;

    public MultipleOfRule(decimal divisor, string? message = null)
    {
        _divisor = divisor;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(decimal value, ValidationContext<TContext> context)
    {
        return new ValueTask<ValidationError?>(
            NumericValidators.MultipleOf(value, _divisor, context.Path, _message));
    }
}
