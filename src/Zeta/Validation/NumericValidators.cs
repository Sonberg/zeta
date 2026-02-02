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

    // Double-specific validations
    public static ValidationError? Positive(double value, string path, string? message = null)
        => value > 0
            ? null
            : new ValidationError(path, "positive", message ?? "Must be positive");

    public static ValidationError? Negative(double value, string path, string? message = null)
        => value < 0
            ? null
            : new ValidationError(path, "negative", message ?? "Must be negative");

    public static ValidationError? Finite(double value, string path, string? message = null)
        => !double.IsNaN(value) && !double.IsInfinity(value)
            ? null
            : new ValidationError(path, "finite", message ?? "Must be a finite number");

    // Decimal-specific validations
    public static ValidationError? Positive(decimal value, string path, string? message = null)
        => value > 0
            ? null
            : new ValidationError(path, "positive", message ?? "Must be positive");

    public static ValidationError? Negative(decimal value, string path, string? message = null)
        => value < 0
            ? null
            : new ValidationError(path, "negative", message ?? "Must be negative");

    public static ValidationError? Precision(decimal value, int maxDecimalPlaces, string path, string? message = null)
        => GetDecimalPlaces(value) <= maxDecimalPlaces
            ? null
            : new ValidationError(path, "precision", message ?? $"Must have at most {maxDecimalPlaces} decimal places");

    public static ValidationError? MultipleOf(decimal value, decimal divisor, string path, string? message = null)
        => value % divisor == 0
            ? null
            : new ValidationError(path, "multiple_of", message ?? $"Must be a multiple of {divisor}");

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
