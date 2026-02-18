using Zeta.Core;

namespace Zeta.Rules.Enum;

/// <summary>
/// Validates that an enum value is one of the allowed values.
/// </summary>
public readonly struct OneOfRule<TEnum> : IValidationRule<TEnum>
    where TEnum : struct, System.Enum
{
    private readonly HashSet<TEnum> _allowed;
    private readonly string? _message;

    public OneOfRule(IReadOnlyCollection<TEnum> allowed, string? message = null)
    {
        _allowed = new HashSet<TEnum>(allowed);
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(TEnum value, ValidationContext context)
    {
        var error = _allowed.Contains(value)
            ? null
            : new ValidationError(
                context.Path,
                "enum_one_of",
                _message ?? $"Value '{value}' is not an allowed enum value");

        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: validates that an enum value is one of the allowed values.
/// </summary>
public readonly struct OneOfRule<TEnum, TContext> : IValidationRule<TEnum, TContext>
    where TEnum : struct, System.Enum
{
    private readonly HashSet<TEnum> _allowed;
    private readonly string? _message;

    public OneOfRule(IReadOnlyCollection<TEnum> allowed, string? message = null)
    {
        _allowed = new HashSet<TEnum>(allowed);
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(TEnum value, ValidationContext<TContext> context)
    {
        var error = _allowed.Contains(value)
            ? null
            : new ValidationError(
                context.Path,
                "enum_one_of",
                _message ?? $"Value '{value}' is not an allowed enum value");

        return ValueTaskHelper.FromResult(error);
    }
}

