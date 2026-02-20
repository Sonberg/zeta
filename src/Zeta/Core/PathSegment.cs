namespace Zeta;

internal enum PathSegmentKind { Property, Index, DictionaryKey }

internal readonly struct PathSegment
{
    private readonly PathSegmentKind _kind;
    private readonly string? _name;
    private readonly int _index;
    private readonly object? _key;

    private PathSegment(PathSegmentKind kind, string? name, int index, object? key)
    { _kind = kind; _name = name; _index = index; _key = key; }

    public static PathSegment Property(string name) => new(PathSegmentKind.Property, name, 0, null);
    public static PathSegment Index(int index)      => new(PathSegmentKind.Index, null, index, null);
    public static PathSegment DictionaryKey(object key) => new(PathSegmentKind.DictionaryKey, null, 0, key);

    public PathSegmentKind Kind       => _kind;
    public string?         Name       => _name;
    public int             IndexValue => _index;
    public object?         Key        => _key;
}

/// <summary>
/// Immutable linked list of path segments. Renders lazily to string; cached per node.
/// </summary>
internal sealed class ValidationPath
{
    private readonly ValidationPath? _parent;
    private readonly PathSegment _segment;
    
    private ValidationPath() { }   // root

    private ValidationPath(ValidationPath parent, PathSegment segment)
    { _parent = parent; _segment = segment; }

    public static readonly ValidationPath Root = new();

    public ValidationPath Append(PathSegment segment) => new(this, segment);

    /// <summary>Renders the full path as a string with the provided formatting options.</summary>
    public string Render(PathFormattingOptions options) => BuildString(options);

    private string BuildString(PathFormattingOptions options)
    {
        if (_parent is null)
            return string.Empty;

        var parent = _parent.Render(options);
        return _segment.Kind switch
        {
            PathSegmentKind.Property =>
                string.IsNullOrEmpty(parent)
                    ? options.PropertyNameFormatter(_segment.Name!)
                    : string.Concat(parent, ".", options.PropertyNameFormatter(_segment.Name!)),
            PathSegmentKind.Index =>
                string.IsNullOrEmpty(parent)
                    ? $"[{_segment.IndexValue}]"
                    : $"{parent}[{_segment.IndexValue}]",
            PathSegmentKind.DictionaryKey =>
                string.IsNullOrEmpty(parent)
                    ? $"[{FormatDictionaryKey(options)}]"
                    : $"{parent}[{FormatDictionaryKey(options)}]",
            _ => parent
        };
    }

    private string FormatDictionaryKey(PathFormattingOptions options)
    {
        if (_segment.Key is null)
            return string.Empty;

        return options.DictionaryKeyFormatter(_segment.Key);
    }
}
