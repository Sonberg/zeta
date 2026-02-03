using Zeta.Core;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string has at least a minimum length.
/// </summary>
public readonly struct MinLengthRule : IValidationRule<string>
{
    private readonly int _min;
    private readonly string? _message;

    public MinLengthRule(int min, string? message = null)
    {
        _min = min;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        var error = value.Length >= _min
            ? null
            : new ValidationError(context.Path, "min_length", _message ?? $"Must be at least {_min} characters long");

        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a string has at least a minimum length.
/// </summary>
public readonly struct MinLengthRule<TContext> : IValidationRule<string, TContext>
{
    private readonly int _min;
    private readonly string? _message;

    public MinLengthRule(int min, string? message = null)
    {
        _min = min;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        var error = value.Length >= _min
            ? null
            : new ValidationError(context.Path, "min_length", _message ?? $"Must be at least {_min} characters long");

        return ValueTaskHelper.FromResult(error);
    }
}
