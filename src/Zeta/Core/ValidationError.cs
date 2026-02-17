namespace Zeta;

/// <summary>
/// Represents a single validation error with path, code, and message.
/// </summary>
public sealed record ValidationError
{
    /// <summary>
    /// JSONPath to the invalid value (e.g., "$.user.address.street", "$[0].name").
    /// </summary>
    public string Path { get; } = "$";

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
    public ValidationError(string? path, string code, string message)
    {
        Path = NormalizePath(path);
        Code = code;
        Message = message;
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "$";

        if (path[0] == '$')
            return path;

        if (path[0] == '[')
            return "$" + path;

        return "$." + path;
    }
}
