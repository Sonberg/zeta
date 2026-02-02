using Zeta.Core;
using Zeta.Validation;

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
        return new ValueTask<ValidationError?>(
            StringValidators.MinLength(value, _min, context.Path, _message));
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
        return new ValueTask<ValidationError?>(
            StringValidators.MinLength(value, _min, context.Path, _message));
    }
}
