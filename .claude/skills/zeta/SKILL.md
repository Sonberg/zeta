---
name: zeta
description: Use the Zeta validation library — build schemas with the Z entry point, validate with ValidateAsync, read Result/ValidationError, and wire up ASP.NET Core / FastEndpoints / Blazor. Use when writing or reviewing validation code that consumes Zeta (Z.Object, Z.String, .Field, .Refine, .Using, WithValidation, ZetaPreProcessor).
---

# Using Zeta

Composable, type-safe, async-first validation for .NET. Schemas are immutable (every fluent
method returns a new instance). No exceptions for validation failures — everything returns a `Result`.

`using Zeta;` gives you the `Z` entry point and all the fluent extension methods.

## Build a schema

Everything starts contextless from `Z`:

```csharp
Z.String()  Z.Int()  Z.Double()  Z.Decimal()  Z.Bool()  Z.Guid()
Z.DateTime()  Z.DateOnly()  Z.TimeOnly()  Z.Enum<TEnum>()
Z.Schema<T>()        // T : class   (Z.Schema<T>() is an alias)
Z.Collection<TElement>()             // or Z.Collection(elementSchema) for complex elements
Z.Dictionary<TKey, TValue>()         // or Z.Dictionary(keySchema, valueSchema)
```

Chain validators (they return the schema for chaining):

```csharp
var email = Z.String().Email().MinLength(5).MaxLength(100);
var age   = Z.Int().Min(18).Max(120);
var price = Z.Decimal().Positive().Precision(2);
```

Objects use `.Property(selector, configure)` — property name is auto-camelCased into the error path
(`$.email`, `$.address.street`):

```csharp
var schema = Z.Schema<User>()
    .Property(u => u.Email, s => s.Email().MinLength(5))
    .Property(u => u.Age, s => s.Min(18).Max(100))
    .Property(u => u.Bio, s => s.MaxLength(500).Nullable())   // string? — call .Nullable()
    .Property(u => u.OptionalAge, s => s.Min(0).Nullable())   // int? — call .Nullable() to allow null
    .Property(u => u.Address, addressSchema);                 // reuse a pre-built nested schema
```

Collections use `.Each()` for elements plus collection-level rules:

```csharp
Z.Schema<User>()
    .Property(u => u.Roles, r => r.Each(x => x.MinLength(3)).MinLength(1).MaxLength(10));

Z.Collection<string>().Each(s => s.Email()).MinLength(1);
Z.Collection(orderItemSchema).MinLength(1);   // complex element type
```

## Nullable / required

Required (non-null) by default. To allow null: `.Nullable()` on the schema — required uniformly for
value-type fields (`int?`, `decimal?`, `Guid?`, …) and reference-type fields (`string?`) alike.
Without it, null fails with `null_value`.

## Conditionals — `.If()`

```csharp
Z.Int().If(v => v >= 18, s => s.Max(65));
Z.Schema<User>().If(u => u.Type == "admin", s => s.Property(u => u.Name, n => n.MinLength(5)));
```

Type narrowing on object schemas:

```csharp
var catSchema = Z.Schema<Cat>().Property(x => x.ClawSharpness, x => x.Min(1).Max(10));
Z.Schema<IAnimal>()
    .If(x => x is Dog, dogSchema)
    .If(x => x is Cat, catSchema);
```

## Custom rules — `.Refine()`

```csharp
Z.String().Refine(v => !v.Contains(' '), "No spaces");   // Func<T,bool>, contextless
```

## Context-aware validation — `.Using<TContext>()`

Promote a contextless schema when you need external data (DB, services). Existing rules/fields/conditionals are carried over. Refinements now take `(value, ctx)`:

```csharp
Z.String()
    .Email()
    .Using<UserContext>()
    .Refine((email, ctx) => email != ctx.BannedEmail, "Email banned")
    .RefineAsync(async (email, ctx, ct) => !await ctx.Repo.ExistsAsync(email, ct), "Email exists");
```

Embed context creation with a factory so the schema stays usable as `ISchema<T>`:

```csharp
Z.Schema<CreateOrderRequest>()
    .Using<OrderContext>(async (value, sp, ct) => new OrderContext {
        HasAccess = await sp.GetRequiredService<IPermissionService>().CheckAccessAsync(value.CustomerId, ct)
    })
    .Property(x => x.CustomerId, x => x.NotEmpty());
```

## Run validation & read the result

```csharp
var result = await schema.ValidateAsync(model);   // ValueTask<Result<T>> / Result
if (result.IsSuccess)
    Use(result.Value);
else
    foreach (var e in result.Errors)              // ValidationError: Path (structured), PathString, Code, Message
        Console.WriteLine($"{e.PathString}: {e.Message}");   // e.g. $.items[0].name

// or monadic
result.Match(ok => ..., errors => ...);
```

Errors are **aggregated** (no short-circuit) with JSONPath paths. Context-aware schemas that carry a factory can also be validated as `ISchema<T>` (the factory resolves via the `IServiceProvider` on the validation run).

## Integrations

**ASP.NET Core minimal API** — endpoint filter, 400 on failure:

```csharp
builder.Services.AddZeta();
app.MapPost("/users", (CreateUserRequest req) => Results.Ok())
   .WithValidation(Schemas.CreateUser);
```

**FastEndpoints** — pre-processor:

```csharp
public override void Configure() {
    Post("/products");
    PreProcessors(new ZetaPreProcessor<CreateProductRequest>(Schema));
}
```

**Blazor / manual** — inject `IZetaValidator` or call `schema.ValidateAsync(model)` directly and bind `result.Errors` by `Path`.

## Notes

- Prefer pre-built named schemas for reuse across objects; inline `s => ...` builders for one-offs.
- Value-schema validators live in `*SchemaExtensions` classes; if you need a new one, see the "Adding Validation Methods" section of the repo's `CLAUDE.md` (don't add per-context duplicates).
- After adding schema features, add a note under "Next release" in `CHANGELOG.md`.
