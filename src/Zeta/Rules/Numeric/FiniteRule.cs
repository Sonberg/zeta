using Zeta.Core;

namespace Zeta.Rules.Numeric;

/// <summary>
/// Validates that a double value is finite (not NaN or infinity).
/// </summary>
public readonly struct FiniteRule : IValidationRule<double?>
{
    private readonly string? _message;

    public FiniteRule(string? message = null)
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

        var error = !double.IsNaN(value.Value) && !double.IsInfinity(value.Value)
            ? null
            : new ValidationError(context.Path, "finite", _message ?? "Must be a finite number");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a double value is finite (not NaN or infinity).
/// </summary>
public readonly struct FiniteRule<TContext> : IValidationRule<double?, TContext>
{
    private readonly string? _message;

    public FiniteRule(string? message = null)
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

        var error = !double.IsNaN(value.Value) && !double.IsInfinity(value.Value)
            ? null
            : new ValidationError(context.Path, "finite", _message ?? "Must be a finite number");
        return ValueTaskHelper.FromResult(error);
    }
}
