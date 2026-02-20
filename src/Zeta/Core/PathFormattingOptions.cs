namespace Zeta;

/// <summary>
/// Controls how validation paths are rendered.
/// </summary>
public sealed class PathFormattingOptions
{
    /// <summary>
    /// Global default path formatting options.
    /// </summary>
    public static PathFormattingOptions Default { get; } = new();

    /// <summary>
    /// Transforms object property names when rendering paths.
    /// Defaults to lower-casing the first character to preserve existing behavior.
    /// </summary>
    public Func<string, string> PropertyNameFormatter { get; init; } = static name =>
    {
        if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
            return name;
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    };

    /// <summary>
    /// Formats dictionary keys when rendering bracket-notation paths.
    /// </summary>
    public Func<object, string> DictionaryKeyFormatter { get; init; } =
        static key => key.ToString() ?? string.Empty;
}
