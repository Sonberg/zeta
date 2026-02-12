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