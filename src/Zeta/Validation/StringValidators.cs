using System.Text.RegularExpressions;

namespace Zeta.Validation;

/// <summary>
/// Static validation methods for strings.
/// These are shared between contextless and context-aware schemas.
/// </summary>
public static class StringValidators
{
    public static ValidationError? MinLength(string value, int min, string path, string? message = null)
        => value.Length >= min
            ? null
            : new ValidationError(path, "min_length", message ?? $"Must be at least {min} characters long");

    public static ValidationError? MaxLength(string value, int max, string path, string? message = null)
        => value.Length <= max
            ? null
            : new ValidationError(path, "max_length", message ?? $"Must be at most {max} characters long");

    public static ValidationError? Length(string value, int exact, string path, string? message = null)
        => value.Length == exact
            ? null
            : new ValidationError(path, "length", message ?? $"Must be exactly {exact} characters long");

    public static ValidationError? NotEmpty(string value, string path, string? message = null)
        => !string.IsNullOrWhiteSpace(value)
            ? null
            : new ValidationError(path, "required", message ?? "Value cannot be empty");

    public static ValidationError? Email(string value, string path, string? message = null)
        => EmailRegex.IsMatch(value)
            ? null
            : new ValidationError(path, "email", message ?? "Invalid email format");

    public static ValidationError? Uuid(string value, string path, string? message = null)
        => Guid.TryParse(value, out _)
            ? null
            : new ValidationError(path, "uuid", message ?? "Invalid UUID format");

    public static ValidationError? Url(string value, string path, string? message = null)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
           (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? null
            : new ValidationError(path, "url", message ?? "Invalid URL format");

    public static ValidationError? ValidUri(string value, UriKind kind, string path, string? message = null)
        => System.Uri.TryCreate(value, kind, out _)
            ? null
            : new ValidationError(path, "uri", message ?? "Invalid URI format");

    public static ValidationError? Alphanumeric(string value, string path, string? message = null)
        => value.All(char.IsLetterOrDigit)
            ? null
            : new ValidationError(path, "alphanumeric", message ?? "Must contain only letters and numbers");

    public static ValidationError? StartsWith(string value, string prefix, StringComparison comparison, string path, string? message = null)
        => value.StartsWith(prefix, comparison)
            ? null
            : new ValidationError(path, "starts_with", message ?? $"Must start with '{prefix}'");

    public static ValidationError? EndsWith(string value, string suffix, StringComparison comparison, string path, string? message = null)
        => value.EndsWith(suffix, comparison)
            ? null
            : new ValidationError(path, "ends_with", message ?? $"Must end with '{suffix}'");

    public static ValidationError? Contains(string value, string substring, StringComparison comparison, string path, string? message = null)
        => value.IndexOf(substring, comparison) >= 0
            ? null
            : new ValidationError(path, "contains", message ?? $"Must contain '{substring}'");

    public static ValidationError? MatchesRegex(string value, Regex regex, string path, string? message = null, string code = "regex")
        => regex.IsMatch(value)
            ? null
            : new ValidationError(path, code, message ?? $"Must match pattern {regex}");

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));
}
