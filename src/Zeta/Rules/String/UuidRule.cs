using Zeta.Core;
using Zeta.Validation;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string is a valid UUID.
/// </summary>
public readonly struct UuidRule : IValidationRule<string>
{
    private readonly string? _message;

    public UuidRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.Uuid(value, context.Path, _message));
    }
}

/// <summary>
/// Context-aware version: Validates that a string is a valid UUID.
/// </summary>
public readonly struct UuidRule<TContext> : IValidationRule<string, TContext>
{
    private readonly string? _message;

    public UuidRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.Uuid(value, context.Path, _message));
    }
}
