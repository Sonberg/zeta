using Zeta.Core;

namespace Zeta.Rules.Numeric;

/// <summary>
/// Validates that a double value is positive (greater than 0).
/// </summary>
public readonly struct PositiveDoubleRule : IValidationRule<double?>
{
    private readonly string? _message;

    public PositiveDoubleRule(string? message = null)
    {
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

        var error = value.Value > 0
            ? null
            : new ValidationError(context.Path, "positive", _message ?? "Must be positive");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a double value is positive (greater than 0).
/// </summary>
public readonly struct PositiveDoubleRule<TContext> : IValidationRule<double?, TContext>
{
    private readonly string? _message;

    public PositiveDoubleRule(string? message = null)
    {
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

        var error = value.Value > 0
            ? null
            : new ValidationError(context.Path, "positive", _message ?? "Must be positive");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Validates that a decimal value is positive (greater than 0).
/// </summary>
public readonly struct PositiveDecimalRule : IValidationRule<decimal?>
{
    private readonly string? _message;

    public PositiveDecimalRule(string? message = null)
    {
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

        var error = value.Value > 0
            ? null
            : new ValidationError(context.Path, "positive", _message ?? "Must be positive");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a decimal value is positive (greater than 0).
/// </summary>
public readonly struct PositiveDecimalRule<TContext> : IValidationRule<decimal?, TContext>
{
    private readonly string? _message;

    public PositiveDecimalRule(string? message = null)
    {
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

        var error = value.Value > 0
            ? null
            : new ValidationError(context.Path, "positive", _message ?? "Must be positive");
        return ValueTaskHelper.FromResult(error);
    }
}
