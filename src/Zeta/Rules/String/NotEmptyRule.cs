using Zeta.Core;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string is not empty or whitespace.
/// </summary>
public readonly struct NotEmptyRule : IValidationRule<string>
{
    private readonly string? _message;

    public NotEmptyRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        var error = !string.IsNullOrWhiteSpace(value)
            ? null
            : new ValidationError(context.Path, "required", _message ?? "Value cannot be empty");

        return new ValueTask<ValidationError?>(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a string is not empty or whitespace.
/// </summary>
public readonly struct NotEmptyRule<TContext> : IValidationRule<string, TContext>
{
    private readonly string? _message;

    public NotEmptyRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        var error = !string.IsNullOrWhiteSpace(value)
            ? null
            : new ValidationError(context.Path, "required", _message ?? "Value cannot be empty");

        return new ValueTask<ValidationError?>(error);
    }
}
