using Zeta.Core;

namespace Zeta.Rules.Numeric;

/// <summary>
/// Validates that an int value is less than or equal to a maximum.
/// </summary>
public readonly struct MaxIntRule : IValidationRule<int>
{
    private readonly int _max;
    private readonly string? _message;

    public MaxIntRule(int max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(int value, ValidationContext context)
    {
        var error = value <= _max
            ? null
            : new ValidationError(context.Path, "max_value", _message ?? $"Must be at most {_max}");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that an int value is less than or equal to a maximum.
/// </summary>
public readonly struct MaxIntRule<TContext> : IValidationRule<int, TContext>
{
    private readonly int _max;
    private readonly string? _message;

    public MaxIntRule(int max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(int value, ValidationContext<TContext> context)
    {
        var error = value <= _max
            ? null
            : new ValidationError(context.Path, "max_value", _message ?? $"Must be at most {_max}");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a double value is less than or equal to a maximum.
/// </summary>
public readonly struct MaxDoubleRule : IValidationRule<double>
{
    private readonly double _max;
    private readonly string? _message;

    public MaxDoubleRule(double max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(double value, ValidationContext context)
    {
        var error = value <= _max
            ? null
            : new ValidationError(context.Path, "max_value", _message ?? $"Must be at most {_max}");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a double value is less than or equal to a maximum.
/// </summary>
public readonly struct MaxDoubleRule<TContext> : IValidationRule<double, TContext>
{
    private readonly double _max;
    private readonly string? _message;

    public MaxDoubleRule(double max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(double value, ValidationContext<TContext> context)
    {
        var error = value <= _max
            ? null
            : new ValidationError(context.Path, "max_value", _message ?? $"Must be at most {_max}");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a decimal value is less than or equal to a maximum.
/// </summary>
public readonly struct MaxDecimalRule : IValidationRule<decimal>
{
    private readonly decimal _max;
    private readonly string? _message;

    public MaxDecimalRule(decimal max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(decimal value, ValidationContext context)
    {
        var error = value <= _max
            ? null
            : new ValidationError(context.Path, "max_value", _message ?? $"Must be at most {_max}");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a decimal value is less than or equal to a maximum.
/// </summary>
public readonly struct MaxDecimalRule<TContext> : IValidationRule<decimal, TContext>
{
    private readonly decimal _max;
    private readonly string? _message;

    public MaxDecimalRule(decimal max, string? message = null)
    {
        _max = max;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(decimal value, ValidationContext<TContext> context)
    {
        var error = value <= _max
            ? null
            : new ValidationError(context.Path, "max_value", _message ?? $"Must be at most {_max}");
        return ValueTaskHelper.FromResult(error);
    }
}
