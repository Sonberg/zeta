using Zeta.Core;
using Zeta.Validation;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string is a valid URI with specified kind.
/// </summary>
public readonly struct UriRule : IValidationRule<string>
{
    private readonly UriKind _kind;
    private readonly string? _message;

    public UriRule(UriKind kind = UriKind.Absolute, string? message = null)
    {
        _kind = kind;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.ValidUri(value, _kind, context.Path, _message));
    }
}

/// <summary>
/// Context-aware version: Validates that a string is a valid URI with specified kind.
/// </summary>
public readonly struct UriRule<TContext> : IValidationRule<string, TContext>
{
    private readonly UriKind _kind;
    private readonly string? _message;

    public UriRule(UriKind kind = UriKind.Absolute, string? message = null)
    {
        _kind = kind;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.ValidUri(value, _kind, context.Path, _message));
    }
}
