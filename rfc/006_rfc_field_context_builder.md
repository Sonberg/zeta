# RFC 006

**Status:** 🟡 Partially implemented — path tracking via `PathSegment`/`ValidationPath` (Property/Index/DictionaryKey) is done. The per-rule config-builder overload (`.MinLength(5, x => x.Code(...).Path(...).Message(...))`) is **planned / not implemented**; rules currently take only a `string? message`.

Example:

```csharp
Z.String()
    .MinLength(5, x => x.Code("invalid_name").Path("Name").Message("Must be at least 5 characters long"))
```


Track paths:

```csharp
public abstract record PathSegment;

public sealed record PropertySegment(string Name) : PathSegment;

public sealed record IndexSegment(int Index) : PathSegment;

Path = Array.Empty<PathSegment>();
```