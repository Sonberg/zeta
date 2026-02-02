using Zeta.Core;
using Zeta.Validation;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string has an exact length.
/// </summary>
public readonly struct LengthRule : IValidationRule<string>
{
    private readonly int _exact;
    private readonly string? _message;

    public LengthRule(int exact, string? message = null)
    {
        _exact = exact;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.Length(value, _exact, context.Path, _message));
    }
}

/// <summary>
/// Context-aware version: Validates that a string has an exact length.
/// </summary>
public readonly struct LengthRule<TContext> : IValidationRule<string, TContext>
{
    private readonly int _exact;
    private readonly string? _message;

    public LengthRule(int exact, string? message = null)
    {
        _exact = exact;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.Length(value, _exact, context.Path, _message));
    }
}
