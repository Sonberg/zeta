# RFC 006

**Status:** ✅ Implemented — path tracking via `PathSegment`/`ValidationPath` (Property/Index/DictionaryKey) is done. The per-rule error config is shipped as a chained `.WithError(...)` builder (rewrites the code/message/path of the most recently added rule) rather than a second argument on every validator — one method covering all rules instead of N overloads:

```csharp
Z.String()
    .MinLength(5)
    .WithError(x => x.Code("invalid_name").Path("Name").Message("Must be at least 5 characters long"))
```

Original proposal (per-validator overload — not adopted):

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