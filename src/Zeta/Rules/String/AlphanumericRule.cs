using Zeta.Core;
using Zeta.Validation;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string contains only alphanumeric characters.
/// </summary>
public readonly struct AlphanumericRule : IValidationRule<string>
{
    private readonly string? _message;

    public AlphanumericRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.Alphanumeric(value, context.Path, _message));
    }
}

/// <summary>
/// Context-aware version: Validates that a string contains only alphanumeric characters.
/// </summary>
public readonly struct AlphanumericRule<TContext> : IValidationRule<string, TContext>
{
    private readonly string? _message;

    public AlphanumericRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.Alphanumeric(value, context.Path, _message));
    }
}
