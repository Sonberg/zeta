using Zeta.Core;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string ends with a specified suffix.
/// </summary>
public readonly struct EndsWithRule : IValidationRule<string>
{
    private readonly string _suffix;
    private readonly StringComparison _comparison;
    private readonly string? _message;

    public EndsWithRule(string suffix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        _suffix = suffix;
        _comparison = comparison;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        var error = value.EndsWith(_suffix, _comparison)
            ? null
            : new ValidationError(context.Path, "ends_with", _message ?? $"Must end with '{_suffix}'");

        return new ValueTask<ValidationError?>(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a string ends with a specified suffix.
/// </summary>
public readonly struct EndsWithRule<TContext> : IValidationRule<string, TContext>
{
    private readonly string _suffix;
    private readonly StringComparison _comparison;
    private readonly string? _message;

    public EndsWithRule(string suffix, StringComparison comparison = StringComparison.Ordinal, string? message = null)
    {
        _suffix = suffix;
        _comparison = comparison;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        var error = value.EndsWith(_suffix, _comparison)
            ? null
            : new ValidationError(context.Path, "ends_with", _message ?? $"Must end with '{_suffix}'");

        return new ValueTask<ValidationError?>(error);
    }
}
