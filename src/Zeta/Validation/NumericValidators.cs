namespace Zeta.Validation;

/// <summary>
/// Static validation methods for numeric types.
/// These are shared between contextless and context-aware schemas.
/// </summary>
public static class NumericValidators
{
    // Int validations
    public static ValidationError? Min(int value, int min, string path, string? message = null)
        => value >= min
            ? null
            : new ValidationError(path, "min_value", message ?? $"Must be at least {min}");

    public static ValidationError? Max(int value, int max, string path, string? message = null)
        => value <= max
            ? null
            : new ValidationError(path, "max_value", message ?? $"Must be at most {max}");

    // Double validations
    public static ValidationError? Min(double value, double min, string path, string? message = null)
        => value >= min
            ? null
            : new ValidationError(path, "min_value", message ?? $"Must be at least {min}");

    public static ValidationError? Max(double value, double max, string path, string? message = null)
        => value <= max
            ? null
            : new ValidationError(path, "max_value", message ?? $"Must be at most {max}");

    // Decimal validations
    public static ValidationError? Min(decimal value, decimal min, string path, string? message = null)
        => value >= min
            ? null
            : new ValidationError(path, "min_value", message ?? $"Must be at least {min}");

    public static ValidationError? Max(decimal value, decimal max, string path, string? message = null)
        => value <= max
            ? null
            : new ValidationError(path, "max_value", message ?? $"Must be at most {max}");
}
