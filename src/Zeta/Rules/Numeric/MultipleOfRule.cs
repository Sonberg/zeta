using Zeta.Core;

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
        var error = value % _divisor == 0
            ? null
            : new ValidationError(context.PathSegments, "multiple_of", _message ?? $"Must be a multiple of {_divisor}");
        return ValueTaskHelper.FromResult(error);
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
        var error = value % _divisor == 0
            ? null
            : new ValidationError(context.PathSegments, "multiple_of", _message ?? $"Must be a multiple of {_divisor}");
        return ValueTaskHelper.FromResult(error);
    }
}
