using Zeta.Core;
using Zeta.Validation;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string contains a specified substring.
/// </summary>
public readonly struct ContainsRule : IValidationRule<string>
{
    private readonly string _substring;
    private readonly StringComparison _comparison;
    private readonly string? _message;

    public ContainsRule(string substring, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        _substring = substring;
        _comparison = comparison;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.Contains(value, _substring, _comparison, context.Path, _message));
    }
}

/// <summary>
/// Context-aware version: Validates that a string contains a specified substring.
/// </summary>
public readonly struct ContainsRule<TContext> : IValidationRule<string, TContext>
{
    private readonly string _substring;
    private readonly StringComparison _comparison;
    private readonly string? _message;

    public ContainsRule(string substring, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        _substring = substring;
        _comparison = comparison;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.Contains(value, _substring, _comparison, context.Path, _message));
    }
}
