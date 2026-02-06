using Zeta.Core;

namespace Zeta.Rules.Numeric;

/// <summary>
/// Validates that an int value is greater than or equal to a minimum.
/// </summary>
public readonly struct MinIntRule : IValidationRule<int?>
{
    private readonly int _min;
    private readonly string? _message;

    public MinIntRule(int min, string? message = null)
    {
        _min = min;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(int? value, ValidationContext context)
    {
        // Defensive null check - base schema should have already validated this
        if (!value.HasValue)
        {
            return ValueTaskHelper.FromResult<ValidationError?>(
                new ValidationError(context.Path, "required", "This field is required."));
        }

        var error = value.Value >= _min
            ? null
            : new ValidationError(context.Path, "min_value", _message ?? $"Must be at least {_min}");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that an int value is greater than or equal to a minimum.
/// </summary>
public readonly struct MinIntRule<TContext> : IValidationRule<int?, TContext>
{
    private readonly int _min;
    private readonly string? _message;

    public MinIntRule(int min, string? message = null)
    {
        _min = min;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(int? value, ValidationContext<TContext> context)
    {
        // Defensive null check - base schema should have already validated this
        if (!value.HasValue)
        {
            return ValueTaskHelper.FromResult<ValidationError?>(
                new ValidationError(context.Path, "required", "This field is required."));
        }

        var error = value.Value >= _min
            ? null
            : new ValidationError(context.Path, "min_value", _message ?? $"Must be at least {_min}");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a double value is greater than or equal to a minimum.
/// </summary>
public readonly struct MinDoubleRule : IValidationRule<double?>
{
    private readonly double _min;
    private readonly string? _message;

    public MinDoubleRule(double min, string? message = null)
    {
        _min = min;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(double? value, ValidationContext context)
    {
        // Defensive null check - base schema should have already validated this
        if (!value.HasValue)
        {
            return ValueTaskHelper.FromResult<ValidationError?>(
                new ValidationError(context.Path, "required", "This field is required."));
        }

        var error = value.Value >= _min
            ? null
            : new ValidationError(context.Path, "min_value", _message ?? $"Must be at least {_min}");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a double value is greater than or equal to a minimum.
/// </summary>
public readonly struct MinDoubleRule<TContext> : IValidationRule<double?, TContext>
{
    private readonly double _min;
    private readonly string? _message;

    public MinDoubleRule(double min, string? message = null)
    {
        _min = min;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(double? value, ValidationContext<TContext> context)
    {
        // Defensive null check - base schema should have already validated this
        if (!value.HasValue)
        {
            return ValueTaskHelper.FromResult<ValidationError?>(
                new ValidationError(context.Path, "required", "This field is required."));
        }

        var error = value.Value >= _min
            ? null
            : new ValidationError(context.Path, "min_value", _message ?? $"Must be at least {_min}");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a decimal value is greater than or equal to a minimum.
/// </summary>
public readonly struct MinDecimalRule : IValidationRule<decimal?>
{
    private readonly decimal _min;
    private readonly string? _message;

    public MinDecimalRule(decimal min, string? message = null)
    {
        _min = min;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(decimal? value, ValidationContext context)
    {
        // Defensive null check - base schema should have already validated this
        if (!value.HasValue)
        {
            return ValueTaskHelper.FromResult<ValidationError?>(
                new ValidationError(context.Path, "required", "This field is required."));
        }

        var error = value.Value >= _min
            ? null
            : new ValidationError(context.Path, "min_value", _message ?? $"Must be at least {_min}");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a decimal value is greater than or equal to a minimum.
/// </summary>
public readonly struct MinDecimalRule<TContext> : IValidationRule<decimal?, TContext>
{
    private readonly decimal _min;
    private readonly string? _message;

    public MinDecimalRule(decimal min, string? message = null)
    {
        _min = min;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(decimal? value, ValidationContext<TContext> context)
    {
        // Defensive null check - base schema should have already validated this
        if (!value.HasValue)
        {
            return ValueTaskHelper.FromResult<ValidationError?>(
                new ValidationError(context.Path, "required", "This field is required."));
        }

        var error = value.Value >= _min
            ? null
            : new ValidationError(context.Path, "min_value", _message ?? $"Must be at least {_min}");
        return ValueTaskHelper.FromResult(error);
    }
}
