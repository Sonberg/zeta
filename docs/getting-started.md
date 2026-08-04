# Getting started

## Install

```bash
dotnet add package Zeta
```

Zeta targets **.NET 6 and later** and has no dependencies. If you're validating HTTP requests, add
one of the integration packages as well:

```bash
# Minimal APIs and Controllers
dotnet add package Zeta.AspNetCore

# FastEndpoints
dotnet add package Zeta.FastEndpoints
```

Both integration packages require .NET 8 or later.

### Which package do I need?

| Package | Use it when |
|---|---|
| `Zeta` | Any .NET app — console, worker, class library, Blazor, MAUI |
| `Zeta.AspNetCore` | You want endpoint-level validation for Minimal APIs or Controllers |
| `Zeta.FastEndpoints` | You use [FastEndpoints](https://fast-endpoints.com) as your web framework |

Blazor and MAUI apps normally need only `Zeta`.

## Your first schema

A schema describes what valid data looks like. Build one with the static `Z` entry point:

```csharp
using Zeta;

var userSchema = Z.Schema<User>()
    .Property(u => u.Name, s => s.MinLength(2).MaxLength(50))
    .Property(u => u.Email, s => s.Email())
    .Property(u => u.Age, s => s.Min(18).Max(120));

public sealed record User(string Name, string Email, int Age);
```

`.Property()` takes an expression pointing at the member, plus a builder function that adds rules to
that member's schema. The expression is how Zeta learns the property name for error paths — it is
never compiled or invoked as a getter chain at validation time.

## Validate

```csharp
var result = await userSchema.ValidateAsync(new User("Al", "not-an-email", 15));
```

`ValidateAsync` always returns a [`Result<T>`](/results). It never throws for invalid input:

```csharp
if (result.IsSuccess)
{
    User valid = result.Value;
    // ...
}
else
{
    foreach (var error in result.Errors)
        Console.WriteLine($"{error.PathString}  {error.Code}  {error.Message}");
}
```

For the input above that prints:

```
$.email  email  Invalid email format
$.age    min_value  Must be at least 18
```

Two things to notice:

- **Errors are aggregated, not short-circuited.** You get every failure in one pass, which is what an
  API response needs.
- **Paths are JSONPath and camel-cased by default**, so `$.email` lines up with the JSON your client
  actually sent. See [Validation paths](/paths) to align this with a custom naming policy.

## Schemas are values

A schema is immutable. Every fluent call returns a new instance, so a schema is safe to store in a
static field and share across threads and requests:

```csharp
public static class Schemas
{
    public static readonly ISchema<User> User = Z.Schema<User>()
        .Property(u => u.Email, s => s.Email());
}
```

That also means schemas compose. Build a small one and nest it:

```csharp
var addressSchema = Z.Schema<Address>()
    .Property(a => a.Street, s => s.MinLength(3))
    .Property(a => a.ZipCode, s => s.Regex(@"^\d{5}$"));

var customerSchema = Z.Schema<Customer>()
    .Property(c => c.Name, s => s.MinLength(2))
    .Property(c => c.Address, addressSchema);   // reuse
```

Errors from the nested schema keep their full path — `$.address.zipCode`.

## Optional values

Everything is **required by default**. Call `.Nullable()` to allow null:

```csharp
Z.Schema<User>()
    .Property(u => u.Bio, s => s.MaxLength(500).Nullable())  // string? — null is allowed
    .Property(u => u.Age, s => s.Min(0).Max(120));           // int?    — null skips the rules
```

Nullable **value types** (`int?`, `Guid?`, `DateTime?`) skip validation automatically when null — no
`.Nullable()` needed. Nullable **reference types** (`string?`) need the explicit call, because the
compiler's nullability annotation isn't available at runtime.

## Add a rule of your own

`.Refine()` takes any predicate:

```csharp
Z.Schema<User>()
    .Property(u => u.Email, s => s.Email())
    .Refine(u => u.Email != u.Name, "Email cannot equal name", "email_equals_name");
```

The third argument is the error **code** — a stable, machine-readable string your client can branch
on. It defaults to `custom_error`. See [Custom rules](/custom-rules) for async refinements and for
packaging rules as reusable extension methods.

## Where to next

- [Schema types](/schemas) — the full tour of what `Z` can build
- [Validator reference](/validators) — every built-in rule, by type
- [Results and errors](/results) — working with `Result<T>` and `ValidationError`
- [Context-aware validation](/validation-run) — async lookups and dependency injection
- [ASP.NET Core](/aspnetcore) — wire validation into your endpoints
