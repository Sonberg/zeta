using System.Text.RegularExpressions;
using Zeta.Core;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string matches a regular expression pattern.
/// </summary>
public readonly struct RegexRule : IValidationRule<string>
{
    private readonly Regex _regex;
    private readonly string? _message;
    private readonly string _code;

    public RegexRule(Regex regex, string? message = null, string code = "regex")
    {
        _regex = regex;
        _message = message;
        _code = code;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        var error = _regex.IsMatch(value)
            ? null
            : new ValidationError(context.PathSegments, _code, _message ?? $"Must match pattern {_regex}");

        return ValueTask.FromResult(error);
    }
}
