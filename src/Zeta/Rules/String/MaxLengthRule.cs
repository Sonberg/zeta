using Zeta.Core;

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
        var error = value.Length <= _max
            ? null
            : new ValidationError(context.PathSegments, "max_length", _message ?? $"Must be at most {_max} characters long");

        return ValueTaskHelper.FromResult(error);
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
        var error = value.Length <= _max
            ? null
            : new ValidationError(context.PathSegments, "max_length", _message ?? $"Must be at most {_max} characters long");

        return ValueTaskHelper.FromResult(error);
    }
}
