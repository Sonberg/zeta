using Zeta.Core;

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
        var error = Uri.TryCreate(value, _kind, out _)
            ? null
            : new ValidationError(context.Path, "uri", _message ?? "Invalid URI format");

        return new ValueTask<ValidationError?>(error);
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
        var error = Uri.TryCreate(value, _kind, out _)
            ? null
            : new ValidationError(context.Path, "uri", _message ?? "Invalid URI format");

        return new ValueTask<ValidationError?>(error);
    }
}
