# Validation Paths

This guide explains Zeta's structured validation path model and how to format paths consistently.

---

## Path Model

Each `ValidationError` contains:

- `Path` (`ValidationPath`) - structured path segments
- `PathString` (`string`) - rendered JSONPath-like text (for logs/HTTP responses)

```csharp
var result = await schema.ValidateAsync(input);

foreach (var error in result.Errors)
{
    ValidationPath path = error.Path;
    string pathText = error.PathString; // e.g. "$.items[0].quantity"
}
```

Use `Path` for programmatic operations, and `PathString` for display/serialization.

---

## Formatting Paths

Use `PathFormattingOptions` to control rendered property and dictionary-key names:

```csharp
var context = new ValidationContext(
    pathFormattingOptions: new PathFormattingOptions
    {
        PropertyNameFormatter = static name => name.ToLowerInvariant(),
        DictionaryKeyFormatter = static key => $"<{key}>"
    });
```

This affects rendered path text (`PathString`, `ValidationContext.Path`), not rule behavior.

---

## ASP.NET Core Naming Alignment

`ValidationContextBuilder` can infer path formatting from `JsonOptions`:

```csharp
services.AddOptions<JsonOptions>()
    .Configure(o =>
    {
        o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        o.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.KebabCaseLower;
    });
```

When Zeta builds a context from DI, paths follow those policies unless explicitly overridden.

You can always override:

```csharp
var result = await validator.ValidateAsync(
    input,
    schema,
    b => b.WithPathFormatting(new PathFormattingOptions
    {
        PropertyNameFormatter = static n => n,
        DictionaryKeyFormatter = static k => k.ToString() ?? string.Empty
    }));
```

---

## Resolving Values from a Path

`ValidationPath` can locate a value from an object graph:

```csharp
var path = ValidationPath.Parse("$.items[1].quantity");

if (path.TryGetValue(model, out var value))
{
    Console.WriteLine(value);
}
```

This is useful for tooling, diagnostics, and mapping errors back to source values.

---

## Best Practices

1. Build errors with `ValidationPath` directly in library code when available.
2. Avoid string roundtrips (`string -> parse -> string`) in hot paths.
3. Standardize formatting at app boundaries (for example, via `JsonOptions` in ASP.NET Core).
