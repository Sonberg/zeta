using Zeta.Core;
using Zeta.Validation;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string is not empty or whitespace.
/// </summary>
public readonly struct NotEmptyRule : IValidationRule<string>
{
    private readonly string? _message;

    public NotEmptyRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.NotEmpty(value, context.Path, _message));
    }
}

/// <summary>
/// Context-aware version: Validates that a string is not empty or whitespace.
/// </summary>
public readonly struct NotEmptyRule<TContext> : IValidationRule<string, TContext>
{
    private readonly string? _message;

    public NotEmptyRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.NotEmpty(value, context.Path, _message));
    }
}
