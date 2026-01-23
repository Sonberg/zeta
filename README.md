# Zeta

A composable, type-safe, async-first validation framework for .NET inspired by [Zod](https://zod.dev/).

## Features

- **Schema-first** — Define validation as reusable schema objects
- **Async by default** — Every rule can be async, no separate sync/async paths
- **Result pattern** — No exceptions for validation failures
- **Composable** — Schemas are values that can be reused and combined
- **Path-aware errors** — Errors include location (`user.address.street`, `items[0]`)
- **ASP.NET Core native** — First-class support for Minimal APIs and Controllers

## Installation

```bash
dotnet add package Zeta
dotnet add package Zeta.AspNetCore  # For ASP.NET Core integration
```

## Quick Start

```csharp
using Zeta;

// Define a schema
var UserSchema = Z.Object<User>()
    .Field(u => u.Email, Z.String().Email())
    .Field(u => u.Age, Z.Int().Min(18));

// Validate
var result = await UserSchema.ValidateAsync(user);

if (result.IsSuccess)
{
    Console.WriteLine($"Valid: {result.Value}");
}
else
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"{error.Path}: {error.Message}");
    }
}
```

## Schema Types

### String

```csharp
Z.String()
    .MinLength(3)
    .MaxLength(100)
    .Email()
    .Regex(@"^[A-Z]")
    .NotEmpty()
    .Refine(s => s.StartsWith("A"), "Must start with A")
```

### Int

```csharp
Z.Int()
    .Min(0)
    .Max(100)
    .Refine(n => n % 2 == 0, "Must be even")
```

### Double

```csharp
Z.Double()
    .Min(0.0)
    .Max(100.0)
    .Positive()
    .Negative()
    .Finite()
```

### Decimal

```csharp
Z.Decimal()
    .Min(0m)
    .Max(1000m)
    .Positive()
    .Precision(2)       // Max 2 decimal places
    .MultipleOf(0.25m)  // Must be multiple of step
```

### Object

```csharp
Z.Object<User>()
    .Field(u => u.Name, Z.String().MinLength(2))
    .Field(u => u.Email, Z.String().Email())
    .Field(u => u.Address, AddressSchema)  // Nested schemas
    .Refine((u, _) => u.Password != u.Email, "Password cannot be email")
```

### Array

```csharp
Z.Array(Z.String().Email())
    .MinLength(1)
    .MaxLength(10)
    .NotEmpty()

// Errors include index path: "emails[0]", "items[2].name"
```

### List

```csharp
Z.List(Z.Int().Min(0))
    .MinLength(1)
    .MaxLength(100)
    .NotEmpty()
```

## Result Pattern

Validation returns `Result<T>` instead of throwing exceptions:

```csharp
var result = await schema.ValidateAsync(value);

// Pattern match
var output = result.Match(
    success: value => $"Valid: {value}",
    failure: errors => $"Invalid: {errors.Count} errors"
);

// Chain operations
var finalResult = await UserSchema.ValidateAsync(input)
    .Then(user => SaveUserAsync(user))
    .Map(saved => new UserResponse(saved.Id));

// Get value or throw
var user = result.GetOrThrow();

// Get value or default
var user = result.GetOrDefault(defaultUser);
```

## ASP.NET Core Integration

### Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register Zeta services (includes IZetaValidator)
builder.Services.AddZeta();

builder.Services.AddControllers();
```

### Minimal APIs

```csharp
var UserSchema = Z.Object<User>()
    .Field(u => u.Email, Z.String().Email())
    .Field(u => u.Name, Z.String().MinLength(3));

app.MapPost("/users", (User user) => Results.Ok(user))
    .WithValidation(UserSchema);
```

Validation failures return `400 Bad Request` with `ValidationProblemDetails`:

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

### Controllers

Inject `IZetaValidator` for manual validation:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IZetaValidator _validator;

    private static readonly ISchema<User> UserSchema = Z.Object<User>()
        .Field(u => u.Name, Z.String().MinLength(3))
        .Field(u => u.Email, Z.String().Email());

    public UsersController(IZetaValidator validator)
    {
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        var result = await _validator.ValidateAsync(user, UserSchema);

        return result.ToActionResult(valid => Ok(new
        {
            Message = "User created",
            User = valid
        }));
    }
}
```

### Result Extensions

```csharp
// For Controllers - returns Ok or BadRequest
result.ToActionResult()
result.ToActionResult(v => CreatedAtAction(...))

// For Minimal APIs - returns Results.Ok or Results.ValidationProblem
result.ToResult()
result.ToResult(v => Results.Created(...))
```

## Validation Context

For async data loading before validation (e.g., checking database):

### Define Context

```csharp
// Context data loaded before validation
public record UserContext(bool EmailExists, bool IsMaintenanceMode);

// Factory to load context
public class UserContextFactory : IValidationContextFactory<User, UserContext>
{
    private readonly IUserRepository _repo;

    public UserContextFactory(IUserRepository repo) => _repo = repo;

    public async Task<UserContext> CreateAsync(User input, IServiceProvider services, CancellationToken ct)
    {
        return new UserContext(
            EmailExists: await _repo.EmailExistsAsync(input.Email, ct),
            IsMaintenanceMode: await _repo.IsMaintenanceModeAsync(ct)
        );
    }
}
```

### Use Context in Schema

```csharp
var UserSchema = Z.Object<User, UserContext>()
    .Field(u => u.Email, Z.String<UserContext>()
        .Email()
        .Refine((email, ctx) => !ctx.EmailExists, "Email already taken"))
    .Field(u => u.Name, Z.String<UserContext>()
        .MinLength(3)
        .Refine((name, ctx) => !ctx.IsMaintenanceMode, "Registration disabled"));
```

### Register Factory

```csharp
builder.Services.AddZeta(typeof(Program).Assembly);  // Scans for factories
```

## Custom Rules

### Inline Refinement

```csharp
Z.String()
    .Refine(
        value => value.StartsWith("A"),
        message: "Must start with A",
        code: "starts_with_a"
    )
```

### Custom Rule Class

```csharp
public sealed class UniqueEmailRule : IRule<string>
{
    public async ValueTask<ValidationError?> ValidateAsync(
        string value,
        ValidationContext<object?> ctx)
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

## Error Model

```csharp
public record ValidationError(
    string Path,     // "user.address.street" or "items[0]"
    string Code,     // "min_length", "email", "required"
    string Message   // "Must be at least 3 characters"
);
```

### Standard Error Codes

| Code | Meaning |
|------|---------|
| `required` | Value is null/missing |
| `min_length` | Below minimum length |
| `max_length` | Above maximum length |
| `min_value` | Below minimum value |
| `max_value` | Above maximum value |
| `email` | Invalid email format |
| `regex` | Pattern mismatch |
| `precision` | Too many decimal places |
| `positive` | Must be positive |
| `negative` | Must be negative |
| `finite` | Must be finite number |
| `custom_error` | Custom refinement failed |

## Benchmarks

Comparing Zeta against FluentValidation and DataAnnotations on .NET 9 (Apple M2 Pro).

### Valid Input

| Method | Mean | Allocated |
|--------|-----:|----------:|
| FluentValidation | 152 ns | 600 B |
| FluentValidation (Async) | 275 ns | 672 B |
| **Zeta** | **341 ns** | **848 B** |
| DataAnnotations | 686 ns | 1,880 B |

### Invalid Input (with errors)

| Method | Mean | Allocated |
|--------|-----:|----------:|
| **Zeta** | **432 ns** | **1,352 B** |
| DataAnnotations | 1,111 ns | 2,704 B |
| FluentValidation | 2,264 ns | 7,920 B |
| FluentValidation (Async) | 2,446 ns | 7,992 B |

**Key findings:**
- Zeta is **5x faster** than FluentValidation when validation fails
- Zeta uses **6x less memory** than FluentValidation for invalid input
- Zeta is **2x faster** than DataAnnotations in all scenarios
- For valid input, FluentValidation sync is fastest, but Zeta is competitive

Run benchmarks yourself:
```bash
dotnet run --project benchmarks/Zeta.Benchmarks -c Release
```

## Comparison

| Feature | FluentValidation | DataAnnotations | Zeta |
|---------|------------------|-----------------|------|
| Async-first | Partial | No | **Yes** |
| Schema-based | No | No | **Yes** |
| Composable | Limited | No | **Yes** |
| Result pattern | No | No | **Yes** |
| Path-aware errors | Partial | No | **Yes** |
| No exceptions | No | No | **Yes** |

## License

MIT


## Wishlist
- Use ValueTask where possible for better performance
- More built-in rules (e.g., UUID, URL, DateTime)
- Better IDE support for schema definitions (source generators?)
- More ASP.NET Core integration features (filters, model binders)
- Precompiled schema definitions for common types
- Localization support for error messages
- Integration with OpenAPI/Swagger for schema generation
- Support for other .NET platforms (e.g., Xamarin, MAUI)
- ThrowOnFailure option for exceptions instead of Result pattern
- Tests for all methods and edge cases
- Use Linq to validate rules