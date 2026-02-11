# Zeta — Design Outline

A composable, type-safe, async-first validation framework for .NET inspired by Zod.

## Vision

**Schema-first validation** that feels as natural as Zod in TypeScript, but built for modern .NET with async/await and the Result pattern at its core.

```csharp
var UserSchema = Z.Object<User>()
    .Field(u => u.Email, Z.String().Email())
    .Field(u => u.Age, Z.Int().Min(18));

var result = await UserSchema.ValidateAsync(user);
```

## Design Principles

1. **Schema-first** — Not attributes, not fluent validators attached to types
2. **Async by default** — Every rule can be async, no separate sync/async paths
3. **No exceptions for control flow** — Validation failures return Results, never throw
4. **Composable** — Schemas are values that can be reused and combined
5. **Path-aware errors** — Errors know their location (`user.address.street`)
6. **Required by default** — Values must be present unless explicitly marked optional/nullable
7. **Minimal API & MVC native** — First-class integration for both

---

## Final Decisions

| Question | Decision |
|----------|----------|
| Entry point naming | `Z.String()`, `Z.Int()`, `Z.Object<T>()` |
| Nullability | Required by default (Zod-style) |
| Transforms | Included in MVP |

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
    public TResult Match<TResult>(Func<T, TResult> success, Func<IReadOnlyList<ValidationError>, TResult> failure);
}
```

### 3. ISchema<T> and ISchema<T, TContext>

The core abstraction — a schema knows how to validate `T`, optionally with shared context data.

```csharp
// Schema with typed context (for async-loaded data)
public interface ISchema<T, TContext>
{
    Task<Result<T>> ValidateAsync(T value, ValidationContext<TContext> context);
}

// Schema with no context (most common)
public interface ISchema<T> : ISchema<T, object?>
{
    Task<Result<T>> ValidateAsync(T value, ValidationExecutionContext? execution = null);
}
```

### 4. ValidationContext System

Two-layer context system for flexibility:

```csharp
// Execution context: path, services, cancellation
public sealed class ValidationExecutionContext
{
    public string Path { get; }
    public IServiceProvider Services { get; }
    public CancellationToken CancellationToken { get; }
    public ValidationExecutionContext Push(string segment);
}

// Full context: execution + typed data
public readonly struct ValidationContext<TData>
{
    public TData Data { get; }
    public ValidationExecutionContext Execution { get; }
    public ValidationContext<TData> Push(string segment);
}
```

### 5. IValidationContextFactory<TInput, TContext>

Factory for async context creation (e.g., load data from DB before validation).

```csharp
public interface IValidationContextFactory<in TInput, TContext>
{
    Task<TContext> CreateAsync(TInput input, IServiceProvider services, CancellationToken ct);
}
```

### 6. IRule<T, TContext>

Individual validation rule.

```csharp
public interface IRule<in T, TContext>
{
    ValueTask<ValidationError?> ValidateAsync(T value, ValidationContext<TContext> context);
}

// Shorthand for no context
public interface IRule<in T> : IRule<T, object?> { }
```

---

## Schema Types

### Primitive Schemas

```csharp
// String
Z.String()
    .MinLength(3)
    .MaxLength(100)
    .Email()
    .Regex(@"^[A-Z]")
    .NotEmpty()

// Numeric
Z.Int().Min(0).Max(100)

// With context
Z.String<MyContext>()
    .Refine((val, ctx) => val != ctx.ForbiddenValue, "Value not allowed")
```

### Object Schema

```csharp
Z.Object<User>()
    .Field(x => x.Email, Z.String().Email())
    .Field(x => x.Age, Z.Int().Min(18))
    .Field(x => x.Address, AddressSchema)  // Nested schema
    .Refine((u, _) => u.Password != u.Email, "Password cannot be email");

// With context
Z.Object<User, UserContext>()
    .Field(u => u.Email, Z.String<UserContext>()
        .Refine((email, ctx) => !ctx.EmailExists, "Email taken"));
```

---

## Custom Rules

### Sync Refinement

```csharp
Z.String()
    .Refine(
        value => value.StartsWith("A"),
        message: "Must start with A",
        code: "starts_with_a"
    );
```

### Context-Aware Refinement

```csharp
Z.String<UserContext>()
    .Refine(
        (value, ctx) => !ctx.BannedEmails.Contains(value),
        message: "Email is banned",
        code: "email_banned"
    );
```

### Custom Rule Class

```csharp
public sealed class UniqueEmailRule : IRule<string>
{
    public async ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext<object?> ctx)
    {
        var repo = ctx.Execution.Services.GetRequiredService<IUserRepository>();
        var exists = await repo.EmailExistsAsync(value, ctx.Execution.CancellationToken);

        return exists
            ? new ValidationError(ctx.Execution.Path, "email_taken", "Email already taken")
            : null;
    }
}

// Usage
Z.String().Use(new UniqueEmailRule())
```

---

## ASP.NET Core Integration

### Minimal API

```csharp
// Simple validation
app.MapPost("/users", (User user) => Results.Ok(user))
   .WithValidation(UserSchema);

// With context factory (async data loading)
app.MapPost("/users", (User user) => Results.Ok(user))
   .WithValidation(UserAsyncSchema);  // Factory resolved from DI
```

### MVC Controllers

```csharp
// Register globally
builder.Services.AddZetaControllers();

// Register schema in DI
builder.Services.AddSingleton<ISchema<User>>(Z.Object<User>()
    .Field(u => u.Name, Z.String().MinLength(3)));

// Controller - validation runs automatically
[ApiController]
public class UsersController : ControllerBase
{
    [HttpPost]
    public IActionResult Create(User user) => Ok(user);
}
```

### Controller Attributes

```csharp
// Skip validation for a parameter
[HttpPost("import")]
public IActionResult Import([ZetaIgnore] RawData data) => Ok();

// Use a specific schema type
[HttpPost("strict")]
public IActionResult CreateStrict([ZetaValidate(typeof(StrictUserSchema))] User user) => Ok();
```

**Attribute Definitions:**

```csharp
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ZetaIgnoreAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ZetaValidateAttribute : Attribute
{
    public Type SchemaType { get; }
    public ZetaValidateAttribute(Type schemaType) => SchemaType = schemaType;
}
```

### Error Response

Returns `400 Bad Request` with `ValidationProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "email": ["Invalid email format"],
    "name": ["Must be at least 3 characters"]
  }
}
```

---

## Dependency Injection

```csharp
// Register Zeta with context factory scanning
builder.Services.AddZeta(typeof(Program).Assembly);

// Register MVC filter
builder.Services.AddZetaControllers();

// Register schemas
builder.Services.AddSingleton<ISchema<User>>(UserSchema);
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
- Object field: `"email"` (auto-camelCased)
- Nested: `"address.street"`
- Array: `"items[0].name"`

### Standard Error Codes

| Code | Meaning |
|------|---------|
| `required` | Value is null/missing |
| `min_length` | Below minimum length |
| `max_length` | Above maximum length |
| `email` | Invalid email format |
| `min_value` | Below minimum value |
| `max_value` | Above maximum value |
| `regex` | Pattern mismatch |
| `custom_error` | Custom refinement failed |

---

## Project Structure

```
Zeta/
├── src/
│   ├── Zeta/                          # Core library
│   │   ├── Core/
│   │   │   ├── Result.cs
│   │   │   ├── ValidationError.cs
│   │   │   ├── ValidationContext.cs
│   │   │   ├── ValidationExecutionContext.cs
│   │   │   ├── ISchema.cs
│   │   │   ├── IValidationContextFactory.cs
│   │   │   └── SchemaExtensions.cs
│   │   ├── Rules/
│   │   │   └── IRule.cs
│   │   ├── Schemas/
│   │   │   ├── StringSchema.cs
│   │   │   ├── IntSchema.cs
│   │   │   └── ObjectSchema.cs
│   │   └── Zeta.cs                    # Z static entry point
│   │
│   └── Zeta.AspNetCore/               # ASP.NET Core integration
│       ├── Attributes.cs              # [ZetaIgnore], [ZetaValidate]
│       ├── ValidationFilter.cs        # Minimal API filter
│       ├── ZetaValidationActionFilter.cs  # MVC filter
│       ├── Extensions.cs              # WithValidation()
│       └── DependencyInjectionExtensions.cs
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
| Async-first | Partial | No | **Yes** |
| Schema-based | No | No | **Yes** |
| Composable | Limited | No | **Yes** |
| Result pattern | No | No | **Yes** |
| Typed context | No | No | **Yes** |
| Minimal API native | Adapter | Adapter | **Native** |
| MVC native | Adapter | Built-in | **Native** |
| Path-aware errors | Partial | No | **Yes** |
| No exceptions | No | No | **Yes** |

---

## Implemented (v0.1)

- [x] `Result<T>` type with monadic operations
- [x] `ValidationError` and `ValidationContext`
- [x] `ISchema<T>` and `ISchema<T, TContext>` abstractions
- [x] `IRule<T>` and `IRule<T, TContext>` abstractions
- [x] `IValidationContextFactory<TInput, TContext>` for async context
- [x] `StringSchema` with: MinLength, MaxLength, Email, NotEmpty, Regex, Refine
- [x] `IntSchema` with: Min, Max, Refine
- [x] `ObjectSchema<T>` with: Field, Refine (supports nested schemas)
- [x] Minimal API filter: `WithValidation<T>(schema)`
- [x] MVC Controller filter: `AddZetaControllers()`
- [x] Controller attributes: `[ZetaIgnore]`, `[ZetaValidate(typeof(...))]`
- [x] Error aggregation (all errors, no short-circuit)
- [x] 22 passing tests

---

## Future Roadmap

### v0.2
- [ ] Transforms: `.Transform(s => s.Trim())`
- [x] Collection schemas: `Z.Collection<T>()` with `.Each()` for element validation
- [x] Nullable/Optional: `.Nullable()` on schemas, auto-wrapping for nullable value type fields in object schemas

### v0.3
- [ ] Union types
- [x] Dependent field validation (`.When()`) — contextless implemented, context-aware pending
- [ ] OpenAPI integration

### v1.0
- [ ] Localization support
- [x] Source generators for Field overloads and collection extensions
- [ ] Full documentation
