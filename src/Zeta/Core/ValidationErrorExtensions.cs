namespace Zeta;

/// <summary>
/// Extensions for shaping <see cref="ValidationError"/> collections into transport-friendly forms.
/// </summary>
public static class ValidationErrorExtensions
{
    /// <summary>
    /// Groups errors by their JSONPath into a dictionary of path → messages — the shape expected by
    /// ASP.NET Core's <c>ValidationProblem</c> / <c>ValidationProblemDetails</c>.
    /// Note: <see cref="ValidationError.Code"/> is not represented in this shape.
    /// </summary>
    public static Dictionary<string, string[]> ToPathDictionary(this IEnumerable<ValidationError> errors)
        => errors
            .GroupBy(e => e.PathString)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());
}
