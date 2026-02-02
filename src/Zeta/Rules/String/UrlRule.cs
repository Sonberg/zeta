using Zeta.Core;
using Zeta.Validation;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string is a valid URL.
/// </summary>
public readonly struct UrlRule : IValidationRule<string>
{
    private readonly string? _message;

    public UrlRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.Url(value, context.Path, _message));
    }
}

/// <summary>
/// Context-aware version: Validates that a string is a valid URL.
/// </summary>
public readonly struct UrlRule<TContext> : IValidationRule<string, TContext>
{
    private readonly string? _message;

    public UrlRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.Url(value, context.Path, _message));
    }
}
