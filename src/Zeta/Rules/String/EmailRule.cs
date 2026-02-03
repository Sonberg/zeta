using System.Text.RegularExpressions;
using Zeta.Core;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string is a valid email address.
/// </summary>
public readonly struct EmailRule : IValidationRule<string>
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));

    private readonly string? _message;

    public EmailRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        var error = EmailRegex.IsMatch(value)
            ? null
            : new ValidationError(context.Path, "email", _message ?? "Invalid email format");

        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a string is a valid email address.
/// </summary>
public readonly struct EmailRule<TContext> : IValidationRule<string, TContext>
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));

    private readonly string? _message;

    public EmailRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        var error = EmailRegex.IsMatch(value)
            ? null
            : new ValidationError(context.Path, "email", _message ?? "Invalid email format");

        return ValueTaskHelper.FromResult(error);
    }
}
