namespace Zeta.Validation;

/// <summary>
/// Static validation methods for collections.
/// These are shared between contextless and context-aware schemas.
/// </summary>
public static class CollectionValidators
{
    public static ValidationError? MinLength<T>(ICollection<T> value, int min, string path, string? message = null)
        => value.Count >= min
            ? null
            : new ValidationError(path, "min_length", message ?? $"Must have at least {min} items");

    public static ValidationError? MaxLength<T>(ICollection<T> value, int max, string path, string? message = null)
        => value.Count <= max
            ? null
            : new ValidationError(path, "max_length", message ?? $"Must have at most {max} items");

    public static ValidationError? Length<T>(ICollection<T> value, int exact, string path, string? message = null)
        => value.Count == exact
            ? null
            : new ValidationError(path, "length", message ?? $"Must have exactly {exact} items");

    public static ValidationError? NotEmpty<T>(ICollection<T> value, string path, string? message = null)
        => value.Count > 0
            ? null
            : new ValidationError(path, "min_length", message ?? "Must not be empty");

    public static ValidationError? MinCount<T>(List<T> value, int min, string path, string? message = null)
        => value.Count >= min
            ? null
            : new ValidationError(path, "min_length", message ?? $"Must have at least {min} items");

    public static ValidationError? MaxCount<T>(List<T> value, int max, string path, string? message = null)
        => value.Count <= max
            ? null
            : new ValidationError(path, "max_length", message ?? $"Must have at most {max} items");

    public static ValidationError? Count<T>(List<T> value, int exact, string path, string? message = null)
        => value.Count == exact
            ? null
            : new ValidationError(path, "length", message ?? $"Must have exactly {exact} items");

    public static ValidationError? ListNotEmpty<T>(List<T> value, string path, string? message = null)
        => value.Count > 0
            ? null
            : new ValidationError(path, "min_length", message ?? "Must not be empty");
}