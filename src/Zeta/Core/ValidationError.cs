namespace Zeta;

/// <summary>
/// Represents a single validation error with path, code, and message.
/// </summary>
/// <param name="Path">Dot-notation path to the invalid value (e.g., "user.address.street")</param>
/// <param name="Code">Machine-readable error code (e.g., "min_length", "email")</param>
/// <param name="Message">Human-readable error message</param>
public sealed record ValidationError(string Path, string Code, string Message)
{
    /// <summary>
    /// Creates a validation error at the root path.
    /// </summary>
    public static ValidationError Create(string code, string message) => new("", code, message);

    /// <summary>
    /// Creates a new error with the path prepended.
    /// </summary>
    public ValidationError WithPath(string parentPath)
    {
        if (string.IsNullOrEmpty(parentPath))
            return this;
        if (string.IsNullOrEmpty(Path))
            return this with { Path = parentPath };
        return this with { Path = $"{parentPath}.{Path}" };
    }
}
