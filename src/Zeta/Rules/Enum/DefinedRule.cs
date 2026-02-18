using Zeta.Core;

namespace Zeta.Rules.Enum;

/// <summary>
/// Validates that an enum value is defined by the enum type.
/// </summary>
public readonly struct DefinedRule<TEnum> : IValidationRule<TEnum>
    where TEnum : struct, System.Enum
{
    private readonly string? _message;

    public DefinedRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(TEnum value, ValidationContext context)
    {
        var error = System.Enum.IsDefined(typeof(TEnum), value)
            ? null
            : new ValidationError(
                context.Path,
                "enum_defined",
                _message ?? $"Value '{value}' is not defined for enum {typeof(TEnum).Name}");

        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: validates that an enum value is defined by the enum type.
/// </summary>
public readonly struct DefinedRule<TEnum, TContext> : IValidationRule<TEnum, TContext>
    where TEnum : struct, System.Enum
{
    private readonly string? _message;

    public DefinedRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(TEnum value, ValidationContext<TContext> context)
    {
        var error = System.Enum.IsDefined(typeof(TEnum), value)
            ? null
            : new ValidationError(
                context.Path,
                "enum_defined",
                _message ?? $"Value '{value}' is not defined for enum {typeof(TEnum).Name}");

        return ValueTaskHelper.FromResult(error);
    }
}

