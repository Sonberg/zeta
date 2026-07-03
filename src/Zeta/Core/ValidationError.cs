namespace Zeta;

/// <summary>
/// Represents a single validation error with path, code, and message.
/// </summary>
public sealed record ValidationError
{
    /// <summary>
    /// Structured path to the invalid value.
    /// </summary>
    public ValidationPath Path { get; } = ValidationPath.Root;

    /// <summary>
    /// JSONPath representation of <see cref="Path"/>.
    /// </summary>
    public string PathString => Path.ToPathString();

    /// <summary>
    /// Machine-readable error code (e.g., "min_length", "email").
    /// </summary>
    public string Code { get; } = "";

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string Message { get; } = "";

    /// <summary>
    /// The value that was being validated when this error was produced.
    /// Captured directly at the point of failure — no reflection or path round-trip — so it is exact
    /// even when the path uses camel-cased or otherwise reformatted segments.
    /// Only meaningful when <see cref="HasAttemptedValue"/> is <c>true</c>; boxed for value types.
    /// </summary>
    public object? AttemptedValue { get; init; }

    /// <summary>
    /// Whether <see cref="AttemptedValue"/> was captured. Distinguishes a captured <c>null</c> from an
    /// error created without a value (e.g. an aggregate or type-mismatch error).
    /// </summary>
    public bool HasAttemptedValue { get; init; }

    /// <summary>
    /// Tries to get the attempted value as <typeparamref name="TValue"/>.
    /// </summary>
    public bool TryGetAttemptedValue<TValue>(out TValue value)
    {
        if (HasAttemptedValue && AttemptedValue is TValue typed)
        {
            value = typed;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Creates a validation error and normalizes its path to JSONPath.
    /// </summary>
    public ValidationError(ValidationPath? path, string code, string message)
    {
        Path = path ?? ValidationPath.Root;
        Code = code;
        Message = message;
    }

    /// <summary>
    /// Creates a validation error from a JSONPath-like string.
    /// </summary>
    public ValidationError(string? path, string code, string message)
        : this(ValidationPath.Parse(path), code, message)
    {
    }
}
