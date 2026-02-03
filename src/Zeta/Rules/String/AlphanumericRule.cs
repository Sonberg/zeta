using Zeta.Core;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string contains only alphanumeric characters.
/// </summary>
public readonly struct AlphanumericRule : IValidationRule<string>
{
    private readonly string? _message;

    public AlphanumericRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        var error = value.All(char.IsLetterOrDigit)
            ? null
            : new ValidationError(context.Path, "alphanumeric", _message ?? "Must contain only letters and numbers");

        return new ValueTask<ValidationError?>(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a string contains only alphanumeric characters.
/// </summary>
public readonly struct AlphanumericRule<TContext> : IValidationRule<string, TContext>
{
    private readonly string? _message;

    public AlphanumericRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        var error = value.All(char.IsLetterOrDigit)
            ? null
            : new ValidationError(context.Path, "alphanumeric", _message ?? "Must contain only letters and numbers");

        return new ValueTask<ValidationError?>(error);
    }
}
