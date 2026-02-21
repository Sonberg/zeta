using Zeta.Core;

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
        var error = Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? null
            : new ValidationError(context.PathSegments, "url", _message ?? "Invalid URL format");

        return ValueTaskHelper.FromResult(error);
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
        var error = Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? null
            : new ValidationError(context.PathSegments, "url", _message ?? "Invalid URL format");

        return ValueTaskHelper.FromResult(error);
    }
}
