namespace Zeta;

/// <summary>
/// Represents a single validation error with path, code, and message.
/// </summary>
/// <param name="Path">Dot-notation path to the invalid value (e.g., "user.address.street")</param>
/// <param name="Code">Machine-readable error code (e.g., "min_length", "email")</param>
/// <param name="Message">Human-readable error message</param>
public sealed record ValidationError(string Path, string Code, string Message);