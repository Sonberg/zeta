using System.Text;
using System.Reflection;
using System.Collections;

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
public enum ValidationPathSegmentKind
{
    Property,
    Index,
    DictionaryKey
}

/// <summary>
/// Public view of a path segment for custom formatting.
/// </summary>
public readonly record struct ValidationPathSegment(ValidationPathSegmentKind Kind, string? PropertyName, int Index, object? DictionaryKey);

/// <summary>
/// Immutable linked list of path segments. Renders lazily and can resolve values from a root object.
/// </summary>
public sealed class ValidationPath : IEquatable<ValidationPath>
{
    private readonly ValidationPath? _parent;
    private readonly PathSegment _segment;
    private string? _defaultRendered;
    private PathFormattingOptions? _lastOptions;
    private string? _lastRendered;
    
    private ValidationPath() { _defaultRendered = string.Empty; }   // root

    private ValidationPath(ValidationPath parent, PathSegment segment)
    { _parent = parent; _segment = segment; }

    public static readonly ValidationPath Root = new();

    internal ValidationPath Append(PathSegment segment) => new(this, segment);

    /// <summary>Renders the full path as a string with the provided formatting options.</summary>
    internal string Render(PathFormattingOptions options)
    {
        if (ReferenceEquals(options, PathFormattingOptions.Default))
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
        var rendered = Render(formattingOptions ?? PathFormattingOptions.Default);
        if (string.IsNullOrEmpty(rendered))
            return "$";

        return rendered[0] == '[' ? $"${rendered}" : $"$.{rendered}";
    }

    /// <summary>
    /// Converts path to string using a custom segment formatter.
    /// </summary>
    public string ToPathString(Func<ValidationPathSegment, string> segmentFormatter, string root = "$")
    {
        ArgumentNullException.ThrowIfNull(segmentFormatter);

        var segments = CollectSegments();
        var builder = new StringBuilder(root);
        for (var i = 0; i < segments.Length; i++)
            builder.Append(segmentFormatter(ToPublic(segments[i])));

        return builder.ToString();
    }

    /// <summary>
    /// Tries to resolve the value referenced by this path from a given root object.
    /// </summary>
    public bool TryGetValue(object? root, out object? value)
    {
        var current = root;
        var segments = CollectSegments();

        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            if (!TryResolveSegment(current, segment, out current))
            {
                value = null;
                return false;
            }
        }

        value = current;
        return true;
    }

    /// <summary>
    /// Resolves the value referenced by this path and throws if not found.
    /// </summary>
    public object? GetValue(object? root)
    {
        if (!TryGetValue(root, out var value))
            throw new InvalidOperationException($"Cannot resolve value for path '{ToPathString()}'.");

        return value;
    }

    /// <summary>
    /// Returns segments for external formatters.
    /// </summary>
    public IReadOnlyList<ValidationPathSegment> GetSegments()
    {
        var segments = CollectSegments();
        var result = new ValidationPathSegment[segments.Length];
        for (var i = 0; i < segments.Length; i++)
            result[i] = ToPublic(segments[i]);
        return result;
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

    private static ValidationPathSegment ToPublic(PathSegment segment)
        => segment.Kind switch
        {
            PathSegmentKind.Property => new ValidationPathSegment(ValidationPathSegmentKind.Property, segment.Name, -1, null),
            PathSegmentKind.Index => new ValidationPathSegment(ValidationPathSegmentKind.Index, null, segment.IndexValue, null),
            PathSegmentKind.DictionaryKey => new ValidationPathSegment(ValidationPathSegmentKind.DictionaryKey, null, -1, segment.Key),
            _ => default
        };

    private static bool TryResolveSegment(object? current, PathSegment segment, out object? next)
    {
        next = null;
        if (current is null)
            return false;

        switch (segment.Kind)
        {
            case PathSegmentKind.Property:
            {
                var property = current.GetType().GetProperty(segment.Name!, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (property is null)
                    return false;
                next = property.GetValue(current);
                return true;
            }
            case PathSegmentKind.Index:
            {
                if (current is IList list)
                {
                    if (segment.IndexValue < 0 || segment.IndexValue >= list.Count)
                        return false;
                    next = list[segment.IndexValue];
                    return true;
                }

                if (current is Array array)
                {
                    if (segment.IndexValue < 0 || segment.IndexValue >= array.Length)
                        return false;
                    next = array.GetValue(segment.IndexValue);
                    return true;
                }

                return false;
            }
            case PathSegmentKind.DictionaryKey:
            {
                if (current is IDictionary dictionary)
                {
                    if (segment.Key is not null && dictionary.Contains(segment.Key))
                    {
                        next = dictionary[segment.Key];
                        return true;
                    }

                    if (segment.Key is string keyAsString)
                    {
                        foreach (DictionaryEntry entry in dictionary)
                        {
                            if (string.Equals(entry.Key?.ToString(), keyAsString, StringComparison.Ordinal))
                            {
                                next = entry.Value;
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            default:
                return false;
        }
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
