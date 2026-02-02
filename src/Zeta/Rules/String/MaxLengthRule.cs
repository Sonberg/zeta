using Zeta.Core;
using Zeta.Validation;

namespace Zeta.Rules.String;

/// <summary>
/// Validates that a string does not exceed a maximum length.
/// </summary>
public readonly struct MaxLengthRule : IValidationRule<string>
{
    private readonly int _max;
    private readonly string? _message;

    public MaxLengthRule(int max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.MaxLength(value, _max, context.Path, _message));
    }
}

/// <summary>
/// Context-aware version: Validates that a string does not exceed a maximum length.
/// </summary>
public readonly struct MaxLengthRule<TContext> : IValidationRule<string, TContext>
{
    private readonly int _max;
    private readonly string? _message;

    public MaxLengthRule(int max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<TContext> context)
    {
        return new ValueTask<ValidationError?>(
            StringValidators.MaxLength(value, _max, context.Path, _message));
    }
}
