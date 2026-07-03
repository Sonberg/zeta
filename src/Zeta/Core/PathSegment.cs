using System.Text;

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
/// Immutable linked list of path segments. Renders lazily to a JSONPath-like string; cached per node.
/// </summary>
public sealed class ValidationPath : IEquatable<ValidationPath>
{
    private readonly ValidationPath? _parent;
    private readonly PathSegment _segment;
    private readonly PathFormattingOptions _formattingOptions;
    private string? _defaultRendered;
    private PathFormattingOptions? _lastOptions;
    private string? _lastRendered;

    private ValidationPath(PathFormattingOptions formattingOptions)
    {
        _formattingOptions = formattingOptions;
        _defaultRendered = string.Empty;
    }

    private ValidationPath(ValidationPath parent, PathSegment segment)
    {
        _parent = parent;
        _segment = segment;
        _formattingOptions = parent._formattingOptions;
    }

    public static readonly ValidationPath Root = new(PathFormattingOptions.Default);

    public static ValidationPath CreateRoot(PathFormattingOptions formattingOptions)
        => ReferenceEquals(formattingOptions, PathFormattingOptions.Default) ? Root : new ValidationPath(formattingOptions);

    internal ValidationPath Append(PathSegment segment) => new(this, segment);

    internal ValidationPath Concat(ValidationPath suffix)
    {
        if (suffix._parent is null)
            return this;

        var current = this;
        var suffixSegments = suffix.CollectSegments();
        for (var i = 0; i < suffixSegments.Length; i++)
            current = current.Append(suffixSegments[i]);

        return current;
    }

    internal ValidationPath RelativeTo(ValidationPath prefix)
    {
        var pathSegments = CollectSegments();
        var prefixSegments = prefix.CollectSegments();

        if (prefixSegments.Length == 0)
            return this;

        if (pathSegments.Length < prefixSegments.Length)
            return this;

        for (var i = 0; i < prefixSegments.Length; i++)
        {
            if (!AreSame(pathSegments[i], prefixSegments[i]))
                return this;
        }

        var relative = Root;
        for (var i = prefixSegments.Length; i < pathSegments.Length; i++)
            relative = relative.Append(pathSegments[i]);

        return relative;
    }

    /// <summary>Renders the full path as a string with the provided formatting options.</summary>
    internal string Render(PathFormattingOptions options)
    {
        if (ReferenceEquals(options, _formattingOptions))
            return _defaultRendered ??= BuildString(options);

        if (ReferenceEquals(_lastOptions, options))
            return _lastRendered!;

        var rendered = BuildString(options);
        _lastOptions = options;
        _lastRendered = rendered;
        return rendered;
    }

    /// <summary>
    /// Creates a <see cref="ValidationPath"/> from a normalized or non-normalized JSONPath-like string.
    /// </summary>
    public static ValidationPath Parse(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || path == "$")
            return Root;

        var span = path.AsSpan();
        var i = 0;
        if (span[i] == '$')
            i++;

        var current = Root;
        while (i < span.Length)
        {
            if (span[i] == '.')
            {
                i++;
                var start = i;
                while (i < span.Length && span[i] is not '.' and not '[')
                    i++;
                if (i > start)
                    current = current.Append(PathSegment.Property(span[start..i].ToString()));
                continue;
            }

            if (span[i] == '[')
            {
                i++;
                var start = i;
                while (i < span.Length && span[i] != ']')
                    i++;

                var token = i > start ? span[start..i].ToString() : string.Empty;
                if (int.TryParse(token, out var index))
                    current = current.Append(PathSegment.Index(index));
                else
                    current = current.Append(PathSegment.DictionaryKey(token));

                if (i < span.Length && span[i] == ']')
                    i++;
                continue;
            }

            var propertyStart = i;
            while (i < span.Length && span[i] is not '.' and not '[')
                i++;
            if (i > propertyStart)
                current = current.Append(PathSegment.Property(span[propertyStart..i].ToString()));
        }

        return current;
    }

    /// <summary>
    /// Converts path to JSONPath string using optional formatting options.
    /// </summary>
    public string ToPathString(PathFormattingOptions? formattingOptions = null)
    {
        var rendered = Render(formattingOptions ?? _formattingOptions);
        if (string.IsNullOrEmpty(rendered))
            return "$";

        return rendered[0] == '[' ? $"${rendered}" : $"$.{rendered}";
    }

    public override string ToString() => ToPathString();

    public static implicit operator string(ValidationPath path) => path.ToPathString();

    public bool Equals(ValidationPath? other)
        => other is not null && string.Equals(ToPathString(), other.ToPathString(), StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is ValidationPath other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(ToPathString());

    private PathSegment[] CollectSegments()
    {
        var depth = 0;
        for (var current = this; current._parent is not null; current = current._parent)
            depth++;

        var segments = new PathSegment[depth];
        var index = depth - 1;
        for (var current = this; current._parent is not null; current = current._parent)
            segments[index--] = current._segment;

        return segments;
    }

    private static bool AreSame(PathSegment left, PathSegment right)
    {
        if (left.Kind != right.Kind)
            return false;

        return left.Kind switch
        {
            PathSegmentKind.Property => string.Equals(left.Name, right.Name, StringComparison.Ordinal),
            PathSegmentKind.Index => left.IndexValue == right.IndexValue,
            PathSegmentKind.DictionaryKey => Equals(left.Key, right.Key),
            _ => false
        };
    }

    private string BuildString(PathFormattingOptions options)
    {
        if (_parent is null)
            return string.Empty;
        var segments = CollectSegments();

        var builder = new StringBuilder(capacity: 32);
        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            switch (segment.Kind)
            {
                case PathSegmentKind.Property:
                    if (builder.Length > 0)
                        builder.Append('.');
                    builder.Append(options.PropertyNameFormatter(segment.Name!));
                    break;
                case PathSegmentKind.Index:
                    builder.Append('[').Append(segment.IndexValue).Append(']');
                    break;
                case PathSegmentKind.DictionaryKey:
                    builder.Append('[');
                    if (segment.Key is not null)
                        builder.Append(options.DictionaryKeyFormatter(segment.Key));
                    builder.Append(']');
                    break;
            }
        }

        return builder.ToString();
    }
}
