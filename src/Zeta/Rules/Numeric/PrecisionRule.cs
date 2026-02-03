using Zeta.Core;

namespace Zeta.Rules.Numeric;

/// <summary>
/// Validates that a decimal value has at most the specified number of decimal places.
/// </summary>
public readonly struct PrecisionRule : IValidationRule<decimal>
{
    private readonly int _decimals;
    private readonly string? _message;

    public PrecisionRule(int decimals, string? message = null)
    {
        _decimals = decimals;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(decimal value, ValidationContext context)
    {
        var error = GetDecimalPlaces(value) <= _decimals
            ? null
            : new ValidationError(context.Path, "precision", _message ?? $"Must have at most {_decimals} decimal places");
        return new ValueTask<ValidationError?>(error);
    }

    private static int GetDecimalPlaces(decimal value)
    {
        value = Math.Abs(value);
        value -= Math.Truncate(value);
        var places = 0;
        while (value > 0)
        {
            places++;
            value *= 10;
            value -= Math.Truncate(value);
        }
        return places;
    }
}

/// <summary>
/// Context-aware version: Validates that a decimal value has at most the specified number of decimal places.
/// </summary>
public readonly struct PrecisionRule<TContext> : IValidationRule<decimal, TContext>
{
    private readonly int _decimals;
    private readonly string? _message;

    public PrecisionRule(int decimals, string? message = null)
    {
        _decimals = decimals;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(decimal value, ValidationContext<TContext> context)
    {
        var error = GetDecimalPlaces(value) <= _decimals
            ? null
            : new ValidationError(context.Path, "precision", _message ?? $"Must have at most {_decimals} decimal places");
        return new ValueTask<ValidationError?>(error);
    }

    private static int GetDecimalPlaces(decimal value)
    {
        value = Math.Abs(value);
        value -= Math.Truncate(value);
        var places = 0;
        while (value > 0)
        {
            places++;
            value *= 10;
            value -= Math.Truncate(value);
        }
        return places;
    }
}
