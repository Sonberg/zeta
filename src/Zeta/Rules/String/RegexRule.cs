using System.Text.RegularExpressions;
using Zeta.Core;
using Zeta.Validation;

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
        return new ValueTask<ValidationError?>(
            StringValidators.MatchesRegex(value, _regex, context.Path, _message, _code));
    }
}

/// <summary>
/// Context-aware version: Validates that a string matches a regular expression pattern.
/// </summary>
public readonly struct RegexRule<TContext> : IValidationRule<string, TContext>
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

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.MatchesRegex(value, _regex, context.Path, _message, _code));
    }
}
