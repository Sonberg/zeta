using Zeta.Core;

namespace Zeta.Rules;

/// <summary>
/// Validates that a bool value is true.
/// </summary>
public readonly struct IsTrueRule : IValidationRule<bool>
{
    private readonly string? _message;

    public IsTrueRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(bool value, ValidationRun context)
    {
        var error = value
            ? null
            : new ValidationError(context.PathSegments, "is_true", _message ?? "Must be true");
        return ValueTask.FromResult(error);
    }
}

/// <summary>
/// Validates that a bool value is false.
/// </summary>
public readonly struct IsFalseRule : IValidationRule<bool>
{
    private readonly string? _message;

    public IsFalseRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(bool value, ValidationRun context)
    {
        var error = !value
            ? null
            : new ValidationError(context.PathSegments, "is_false", _message ?? "Must be false");
        return ValueTask.FromResult(error);
    }
}
