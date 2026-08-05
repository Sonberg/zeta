# Validator reference

Every built-in rule, grouped by the schema it hangs off. The **code** column is the
`ValidationError.Code` emitted on failure — that's the stable string to branch on in client code.

Every rule takes an optional trailing `string? message` to override the default text:

```csharp
Z.String().MinLength(3, "Name is too short");
```

To change the code as well as the message, use [`.WithError()`](#overriding-code-message-and-path).

## Available on every schema

| Method | Description |
|---|---|
| `.Nullable()` | Allow `null`. Without it, null fails with `null_value` |
| `.Refine(predicate, message, code?)` | Custom synchronous rule. Code defaults to `custom_error` |
| `.RefineAsync(predicate, message, code?)` | Custom async rule, with and without a `CancellationToken` |
| `.If(predicate, builder)` | Apply nested rules only when the predicate holds |
| `.WithError(cfg)` | Override the code, message and/or path of the preceding rule |
| `.Using<TContext>()` | Promote to a context-aware schema |
| `.Using<TContext>(factory)` | Promote and supply the context factory |
| `.ValidateAsync(value)` | Run the schema |

## String — `Z.String()`

| Method | Code | Description |
|---|---|---|
| `.MinLength(n)` | `min_length` | At least `n` characters |
| `.MaxLength(n)` | `max_length` | At most `n` characters |
| `.Length(n)` | `length` | Exactly `n` characters |
| `.NotEmpty()` | `required` | Not null, empty, or whitespace |
| `.Email()` | `email` | Email address format |
| `.Uuid()` | `uuid` | UUID/GUID format |
| `.Url()` | `url` | HTTP/HTTPS URL |
| `.Uri()` | `uri` | Any valid URI |
| `.Alphanumeric()` | `alphanumeric` | Letters and digits only |
| `.StartsWith(s)` | `starts_with` | Has the given prefix |
| `.EndsWith(s)` | `ends_with` | Has the given suffix |
| `.Contains(s)` | `contains` | Contains the substring |
| `.Regex(pattern)` | `regex` | Matches the pattern |

::: warning `.NotEmpty()` means "not whitespace"
A string of spaces fails `.NotEmpty()`. If you only want to reject `""`, use `.MinLength(1)`.
:::

## Int — `Z.Int()`

| Method | Code | Description |
|---|---|---|
| `.Min(n)` | `min_value` | At least `n` |
| `.Max(n)` | `max_value` | At most `n` |
| `.Range(min, max)` | `min_value` / `max_value` | Within the inclusive range |
| `.Positive()` | `positive` | Greater than zero |
| `.Negative()` | `negative` | Less than zero |
| `.MultipleOf(n)` | `multiple_of` | Evenly divisible by `n` |

## Double — `Z.Double()`

| Method | Code | Description |
|---|---|---|
| `.Min(n)` / `.Max(n)` | `min_value` / `max_value` | Bounds |
| `.Range(min, max)` | `min_value` / `max_value` | Inclusive range |
| `.Positive()` / `.Negative()` | `positive` / `negative` | Sign |
| `.MultipleOf(n)` | `multiple_of` | Evenly divisible |
| `.Finite()` | `finite` | Not `NaN`, not ±∞ |

## Decimal — `Z.Decimal()`

| Method | Code | Description |
|---|---|---|
| `.Min(n)` / `.Max(n)` | `min_value` / `max_value` | Bounds |
| `.Range(min, max)` | `min_value` / `max_value` | Inclusive range |
| `.Positive()` / `.Negative()` | `positive` / `negative` | Sign |
| `.MultipleOf(n)` | `multiple_of` | Evenly divisible — useful for price steps |
| `.Precision(n)` | `precision` | At most `n` decimal places |

## DateTime — `Z.DateTime()`

| Method | Code | Description |
|---|---|---|
| `.Min(d)` | `min_date` | Not before `d` |
| `.Max(d)` | `max_date` | Not after `d` |
| `.Between(min, max)` | `between` | Within the range |
| `.Past()` | `past` | Strictly before now |
| `.Future()` | `future` | Strictly after now |
| `.WithinDays(n)` | `within_days` | Within `n` days of now |
| `.Weekday()` | `weekday` | Monday–Friday |
| `.Weekend()` | `weekend` | Saturday–Sunday |
| `.MinAge(n)` | `min_age` | Date is a birthdate of at least `n` years ago |
| `.MaxAge(n)` | `max_age` | …and at most `n` years ago |

## DateOnly — `Z.DateOnly()`

Same as `DateTime` minus `.WithinDays()`: `.Min()`, `.Max()`, `.Between()`, `.Past()`, `.Future()`,
`.Weekday()`, `.Weekend()`, `.MinAge()`, `.MaxAge()`, with the same codes.

## TimeOnly — `Z.TimeOnly()`

| Method | Code | Description |
|---|---|---|
| `.Min(t)` | `min_time` | Not before `t` |
| `.Max(t)` | `max_time` | Not after `t` |
| `.Between(min, max)` | `between` | Within the range |
| `.BusinessHours()` | `business_hours` | 9am–5pm by default; accepts custom bounds |
| `.Morning()` | `morning` | Before noon (12am–12pm) |
| `.Afternoon()` | `afternoon` | 12pm–6pm |
| `.Evening()` | `evening` | 6pm–12am |

## Guid — `Z.Guid()`

| Method | Code | Description |
|---|---|---|
| `.NotEmpty()` | `not_empty` | Not `Guid.Empty` |
| `.Version(n)` | `version` | Specific UUID version (1–5) |

## Bool — `Z.Bool()`

| Method | Code | Description |
|---|---|---|
| `.IsTrue()` | `is_true` | Must be `true` — e.g. an accepted-terms checkbox |
| `.IsFalse()` | `is_false` | Must be `false` |

## Enum — `Z.Enum<TEnum>()`

| Method | Code | Description |
|---|---|---|
| `.Defined()` | `enum_defined` | Value is a declared member of the enum |
| `.OneOf(a, b, …)` | `enum_one_of` | Value is one of the listed members |

## Object — `Z.Schema<T>()`

| Method | Description |
|---|---|
| `.Property(expr, builder)` | Validate a property with an inline builder |
| `.Property(expr, schema)` | Validate a property with a pre-built schema |
| `.Refine(predicate, message, code?)` | Object-level rule, reported at the object's path |
| `.RefineAt(expr, predicate, message)` | Object-level rule reported at one property's path |
| `.As<TDerived>()` | Assert the runtime type. Fails with `type_mismatch` |
| `.If(predicate, builder\|schema)` | Conditional or polymorphic branch |

## Collection — `Z.Collection<T>()`

| Method | Code | Description |
|---|---|---|
| `.Each(builder\|schema)` | — | Validate every element |
| `.MinLength(n)` | `min_length` | At least `n` elements |
| `.MaxLength(n)` | `max_length` | At most `n` elements |
| `.Length(n)` | `length` | Exactly `n` elements |
| `.NotEmpty()` | `min_length` | At least one element |

## Dictionary — `Z.Dictionary<TKey, TValue>()`

| Method | Code | Description |
|---|---|---|
| `.EachKey(schema)` | — | Validate every key |
| `.EachValue(schema)` | — | Validate every value |
| `.RefineEachEntry(predicate, message, code?)` | `entry_invalid` | Rule over the key and value together |
| `.RefineEachEntryAsync(predicate, message, code?)` | `entry_invalid` | Async variant |
| `.MinLength(n)` | `min_length` | At least `n` entries |
| `.MaxLength(n)` | `max_length` | At most `n` entries |
| `.NotEmpty()` | `min_length` | At least one entry |

## Structural codes

These aren't tied to a specific rule:

| Code | Raised when |
|---|---|
| `null_value` | Value is null and the schema isn't `.Nullable()` |
| `type_mismatch` | `.As<T>()` assertion failed |
| `custom_error` | Default code for `.Refine()` when none is given |

## Overriding code, message and path

`.WithError()` rewrites the error produced by the rule immediately before it. Unspecified fields keep
the rule's originals:

```csharp
Z.String()
    .MinLength(8)
    .WithError(e => e
        .Code("password_too_short")
        .Message("Password must be at least 8 characters"));
```

This is the cleanest way to keep a built-in rule's logic while giving your API its own error
vocabulary.

## Adding your own

Rules aren't a closed set — validators are extension methods on `IValueSchema<T, TSelf>`, so your own
sit alongside the built-ins and chain identically:

```csharp
public static TSelf Slug<TSelf>(this IValueSchema<string, TSelf> schema, string? message = null)
    where TSelf : IValueSchema<string, TSelf>
    => schema.Regex("^[a-z0-9-]+$", message ?? "Must be a lowercase slug");
```

Writing it against `IValueSchema<T, TSelf>` rather than a concrete schema type means it works on both
contextless and context-aware string schemas. See [Custom rules](/custom-rules).
