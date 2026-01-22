# Zeta — Design Outline

A composable, type-safe, async-first validation framework for .NET inspired by Zod.

## Vision

**Schema-first validation** that feels as natural as Zod in TypeScript, but built for modern .NET with async/await and the Result pattern at its core.

```csharp
var UserSchema = Zeta.Object<User>()
    .Field(u => u.Email, Zeta.String().Email())
    .Field(u => u.Age, Zeta.Int().Min(18));

var result = await UserSchema.ValidateAsync(user);
```

## Design Principles

1. **Schema-first** — Not attributes, not fluent validators attached to types
2. **Async by default** — Every rule can be async, no separate sync/async paths
3. **No exceptions for control flow** — Validation failures return Results, never throw
4. **Composable** — Schemas are values that can be reused and combined
5. **Path-aware errors** — Errors know their location (`user.address.street`)
6. **Minimal API native** — First-class integration, not an afterthought

---

## Core Abstractions

### 1. ValidationError

```csharp
public sealed record ValidationError(
    string Path,      // "user.email" or "" for root
    string Code,      // "min_length", "email", "required"
    string Message    // Human-readable message
);
```

### 2. Result<T>

Minimal discriminated result type (no external dependencies).

```csharp
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public IReadOnlyList<ValidationError>? Errors { get; }

    public static Result<T> Success(T value);
    public static Result<T> Failure(params ValidationError[] errors);

    // Monadic operations
    public Result<U> Map<U>(Func<T, U> map);
    public Task<Result<U>> Then<U>(Func<T, Task<Result<U>>> bind);
    public T GetOrThrow();
    public T GetOrDefault(T fallback);
}
```

### 3. ISchema<T>

The core abstraction — a schema knows how to validate `T`.

```csharp
public interface ISchema<T>
{
    Task<Result<T>> ValidateAsync(T value, ValidationContext? context = null);
}
```

### 4. ValidationContext

Carries metadata through the validation tree.

```csharp
public sealed class ValidationContext
{
    public string Path { get; }
    public IServiceProvider? Services { get; }
    public CancellationToken CancellationToken { get; }

    public ValidationContext Push(string segment);  // Returns new context with extended path
}
```

### 5. IRule<T>

Individual validation rule.

```csharp
public interface IRule<T>
{
    ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext context);
}
```

---

## Schema Types

### Primitive Schemas

```csharp
// String
Zeta.String()
    .MinLength(3)
    .MaxLength(100)
    .Email()
    .Regex(@"^[A-Z]")
    .NotEmpty()

// Numeric
Zeta.Int().Min(0).Max(100)
Zeta.Long().Positive()
Zeta.Decimal().Precision(2)

// Other primitives
Zeta.Bool()
Zeta.DateTime().Past().After(minDate)
Zeta.Guid().NotEmpty()
```

### Object Schema

```csharp
Zeta.Object<User>()
    .Field(x => x.Email, Zeta.String().Email())
    .Field(x => x.Age, Zeta.Int().Min(18))
    .Field(x => x.Address, AddressSchema)  // Nested schema
    .Refine(u => u.Password != u.Email, "Password cannot be email", "password_equals_email");
```

### Collection Schemas

```csharp
Zeta.Array(Zeta.String().Email())    // string[]
    .MinLength(1)
    .MaxLength(10)

Zeta.List(UserSchema)                 // List<User>
```

### Nullable & Optional

```csharp
Zeta.String().Nullable()              // null is valid
Zeta.String().Optional()              // undefined/missing is valid (for object fields)
```

### Composition

```csharp
// Reuse
var EmailSchema = Zeta.String().Email().MaxLength(255);
var UserSchema = Zeta.Object<User>()
    .Field(x => x.Email, EmailSchema);

// Union (future)
Zeta.Union(Zeta.String(), Zeta.Int())
```

---

## Custom Rules

### Sync Refinement

```csharp
Zeta.String()
    .Refine(
        value => value.StartsWith("A"),
        message: "Must start with A",
        code: "starts_with_a"
    );
```

### Async Refinement (with DI)

```csharp
Zeta.String()
    .RefineAsync(async (value, ctx) =>
    {
        var repo = ctx.Services.GetRequiredService<IUserRepository>();
        return !await repo.EmailExistsAsync(value, ctx.CancellationToken);
    },
    message: "Email already taken",
    code: "email_taken");
```

### Custom Rule Class

```csharp
public sealed class UniqueEmailRule : IRule<string>
{
    public async ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext ctx)
    {
        var repo = ctx.Services.GetRequiredService<IUserRepository>();
        var exists = await repo.EmailExistsAsync(value, ctx.CancellationToken);

        return exists
            ? new ValidationError(ctx.Path, "email_taken", "Email already taken")
            : null;
    }
}

// Usage
Zeta.String().Use(new UniqueEmailRule())
```

---

## Minimal API Integration

### Endpoint Filter

```csharp
app.MapPost("/users", (User user) => Results.Ok(user))
   .WithValidation(UserSchema);
```

### Automatic Error Response

Returns `400 Bad Request` with `ValidationProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "email": ["Invalid email format"],
    "age": ["Must be at least 18"]
  }
}
```

### Result Extensions

```csharp
app.MapPost("/users", async (User user) =>
{
    return await UserSchema.ValidateAsync(user)
        .Then(u => userService.CreateAsync(u))
        .ToMinimalApiResult();  // Maps Result<T> to IResult
});
```

---

## Error Model

### Structure

```csharp
public sealed record ValidationError(
    string Path,      // Dot-notation path: "address.street"
    string Code,      // Machine-readable: "min_length"
    string Message    // Human-readable: "Must be at least 3 characters"
);
```

### Path Building

- Root level: `""`
- Object field: `"email"`
- Nested: `"address.street"`
- Array: `"items[0].name"`

### Standard Error Codes

| Code | Meaning |
|------|---------|
| `required` | Value is null/missing |
| `min_length` | Below minimum length |
| `max_length` | Above maximum length |
| `email` | Invalid email format |
| `min` | Below minimum value |
| `max` | Above maximum value |
| `regex` | Pattern mismatch |
| `refine` | Custom refinement failed |

---

## MVP Scope

### In Scope (v0.1)

- [x] `Result<T>` type with monadic operations
- [x] `ValidationError` and `ValidationContext`
- [x] `ISchema<T>` and `IRule<T>` abstractions
- [x] `StringSchema` with: MinLength, MaxLength, Email, NotEmpty, Regex, Refine, RefineAsync
- [x] `IntSchema` with: Min, Max, Positive, Negative
- [x] `ObjectSchema<T>` with: Field, Refine, RefineAsync
- [x] Minimal API filter: `WithValidation<T>(schema)`
- [x] Error aggregation (all errors, no short-circuit)

### Out of Scope (v0.1)

- Union types
- Transforms / coercion
- Localization
- OpenAPI generation
- Source generators
- Collections beyond basic array
- Dependent field validation (`when`)

---

## Project Structure

```
Zeta/
├── src/
│   ├── Zeta/                      # Core library
│   │   ├── Core/
│   │   │   ├── Result.cs
│   │   │   ├── ValidationError.cs
│   │   │   ├── ValidationContext.cs
│   │   │   └── ISchema.cs
│   │   ├── Rules/
│   │   │   ├── IRule.cs
│   │   │   └── Built-in rules...
│   │   ├── Schemas/
│   │   │   ├── StringSchema.cs
│   │   │   ├── IntSchema.cs
│   │   │   └── ObjectSchema.cs
│   │   └── Zeta.cs                # Static entry point
│   │
│   └── Zeta.AspNetCore/           # Minimal API integration
│       ├── ValidationFilter.cs
│       └── Extensions.cs
│
├── tests/
│   ├── Zeta.Tests/
│   └── Zeta.AspNetCore.Tests/
│
└── samples/
    └── Zeta.Sample.Api/
```

---

## Comparison

| Feature | FluentValidation | DataAnnotations | Zeta |
|---------|------------------|-----------------|------|
| Async-first | Partial | No | Yes |
| Schema-based | No | No | Yes |
| Composable | Limited | No | Yes |
| Result pattern | No | No | Yes |
| Minimal API native | Adapter | Adapter | Native |
| Path-aware errors | Partial | No | Yes |
| No exceptions | No | No | Yes |

---

## Open Questions

1. **Naming**: `Zeta.String()` vs `Schema.String()` vs `Z.String()`?
2. **Nullability**: How to handle `T?` vs required by default?
3. **Transforms**: Should MVP include `.Transform()`?
4. **DI registration**: Auto-discover schemas via assembly scanning?
5. **Error messages**: Built-in vs user-provided vs resource keys?

---

## Next Steps

1. Create solution and project structure
2. Implement `Result<T>` and core types
3. Implement `StringSchema` with basic rules
4. Implement `ObjectSchema<T>`
5. Add Minimal API filter
6. Write tests
7. Create sample API
