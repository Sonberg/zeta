namespace Zeta;

/// <summary>
/// Fluent overrides for the error a single rule produces — its code, message, and reported path.
/// Applied via <c>.WithError(...)</c> to the most recently appended rule (RFC 006).
/// </summary>
public sealed class ErrorConfig
{
    private string? _code;
    private string? _message;
    private string? _path;

    /// <summary>Overrides the machine-readable error code.</summary>
    public ErrorConfig Code(string code) { _code = code; return this; }

    /// <summary>Overrides the human-readable message.</summary>
    public ErrorConfig Message(string message) { _message = message; return this; }

    /// <summary>Overrides the reported path (JSONPath-like string, e.g. "Name" or "$.items[0]").</summary>
    public ErrorConfig Path(string path) { _path = path; return this; }

    /// <summary>Rewrites an error with the configured overrides. Nulls leave the original field untouched.</summary>
    internal ValidationError? Apply(ValidationError? error)
    {
        if (error is null) return null;

        var path = _path is not null ? ValidationPath.Parse(_path) : error.Path;
        return new ValidationError(path, _code ?? error.Code, _message ?? error.Message)
        {
            AttemptedValue = error.AttemptedValue,
            HasAttemptedValue = error.HasAttemptedValue,
        };
    }
}
