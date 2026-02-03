using Zeta.Core;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string has an exact length.
/// </summary>
public readonly struct LengthRule : IValidationRule<string>
{
    private readonly int _exact;
    private readonly string? _message;

    public LengthRule(int exact, string? message = null)
    {
        _exact = exact;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        var error = value.Length == _exact
            ? null
            : new ValidationError(context.Path, "length", _message ?? $"Must be exactly {_exact} characters long");

        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a string has an exact length.
/// </summary>
public readonly struct LengthRule<TContext> : IValidationRule<string, TContext>
{
    private readonly int _exact;
    private readonly string? _message;

    public LengthRule(int exact, string? message = null)
    {
        _exact = exact;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        var error = value.Length == _exact
            ? null
            : new ValidationError(context.Path, "length", _message ?? $"Must be exactly {_exact} characters long");

        return ValueTaskHelper.FromResult(error);
    }
}
