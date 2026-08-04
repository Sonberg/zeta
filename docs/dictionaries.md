# Dictionaries

`Z.Dictionary<TKey, TValue>()` validates keys and values independently, plus the dictionary as a
whole.

```csharp
var scores = Z.Dictionary<string, int>()
    .EachKey(Z.String().MinLength(1).MaxLength(32))
    .EachValue(Z.Int().Min(0).Max(100))
    .NotEmpty();
```

`TKey` must be non-nullable (`where TKey : notnull`), matching `IDictionary<TKey, TValue>`.

## Keys and values

`.EachKey()` and `.EachValue()` each take a pre-built schema:

```csharp
Z.Dictionary<string, decimal>()
    .EachKey(Z.String().Regex("^[a-z-]+$"))
    .EachValue(Z.Decimal().Positive().Precision(2));
```

You can also pass both up front:

```csharp
Z.Dictionary(
    Z.String().NotEmpty(),
    Z.Int().Min(0));
```

Calling `.EachKey()` or `.EachValue()` again replaces the previous schema rather than adding to it —
build the full key or value schema in one chain.

## Dictionary-level rules

| Method | Code | Description |
|---|---|---|
| `.MinLength(n)` | `min_length` | At least `n` entries |
| `.MaxLength(n)` | `max_length` | At most `n` entries |
| `.NotEmpty()` | `min_length` | At least one entry |

```csharp
Z.Dictionary<string, string>()
    .MinLength(1)
    .MaxLength(50);
```

## Rules over a whole entry

When a rule needs the key and value together, use `.RefineEachEntry()`:

```csharp
Z.Dictionary<string, int>()
    .RefineEachEntry(
        (key, value) => key != "total" || value >= 0,
        "Total cannot be negative",
        "negative_total");
```

The code argument is optional and defaults to `entry_invalid`.

The async form additionally receives a `CancellationToken`:

```csharp
Z.Dictionary<string, string>()
    .RefineEachEntryAsync(
        async (key, value, ct) => await _catalog.IsKnownSkuAsync(key, ct),
        "Unknown SKU",
        "unknown_sku");
```

## Error paths

Errors are reported at the entry's key:

```
$.prices['widget']     value failed
$.prices               dictionary-level rule failed
```

How keys are rendered is controlled by `PathFormattingOptions.DictionaryKeyFormatter`. Under
ASP.NET Core this is derived from your `JsonOptions.DictionaryKeyPolicy` so the paths match the JSON
your client sent — see [Validation paths](/paths).

::: tip Non-string keys
Dictionary keys don't have to be strings. `Z.Dictionary<Guid, Order>()` works, and the key is
rendered into the path using the same formatter. Because the key is captured directly rather than
re-parsed from the path text, `ValidationError.AttemptedValue` stays correct for non-string keys.
:::

## In an object schema

```csharp
Z.Schema<Product>()
    .Property(p => p.Translations, Z.Dictionary<string, string>()
        .EachKey(Z.String().Length(2))       // ISO language code
        .EachValue(Z.String().NotEmpty())
        .NotEmpty());
```

Errors nest as you'd expect: `$.translations['en']`.

## Nullable dictionaries

Like every schema, a dictionary is required by default:

```csharp
Z.Schema<Product>()
    .Property(p => p.Metadata, Z.Dictionary<string, string>().Nullable());
```

Note that `.Nullable()` allows the dictionary *itself* to be null. An empty dictionary is a
different thing — reject that with `.NotEmpty()`.
