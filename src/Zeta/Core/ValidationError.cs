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
