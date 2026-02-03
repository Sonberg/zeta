using Zeta.Core;

namespace Zeta.Rules.Numeric;

/// <summary>
/// Validates that a double value is finite (not NaN or infinity).
/// </summary>
public readonly struct FiniteRule : IValidationRule<double>
{
    private readonly string? _message;

    public FiniteRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(double value, ValidationContext context)
    {
        var error = !double.IsNaN(value) && !double.IsInfinity(value)
            ? null
            : new ValidationError(context.Path, "finite", _message ?? "Must be a finite number");
        return ValueTaskHelper.FromResult(error);
    }
}

/// <summary>
/// Context-aware version: Validates that a double value is finite (not NaN or infinity).
/// </summary>
public readonly struct FiniteRule<TContext> : IValidationRule<double, TContext>
{
    private readonly string? _message;

    public FiniteRule(string? message = null)
    {
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(double value, ValidationContext<TContext> context)
    {
        var error = !double.IsNaN(value) && !double.IsInfinity(value)
            ? null
            : new ValidationError(context.Path, "finite", _message ?? "Must be a finite number");
        return ValueTaskHelper.FromResult(error);
    }
}
