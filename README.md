# Zeta

[![GitHub stars](https://img.shields.io/github/stars/Sonberg/zeta.svg?style=social)](https://github.com/Sonberg/zeta/stargazers)

[![codecov](https://codecov.io/gh/Sonberg/zeta/branch/main/graph/badge.svg)](https://codecov.io/gh/Sonberg/zeta) [![NuGet](https://img.shields.io/nuget/v/Zeta.AspNetCore.svg)](https://www.nuget.org/packages/Zeta.AspNetCore) [![NuGet Downloads](https://img.shields.io/nuget/dt/Zeta.AspNetCore.svg)](https://www.nuget.org/packages/Zeta.AspNetCore)
 [![Build](https://github.com/sonberg/zeta/actions/workflows/publish.yml/badge.svg)](https://github.com/Sonberg/zeta/actions) [![Build](https://github.com/sonberg/zeta/actions/workflows/ci.yml/badge.svg)](https://github.com/Sonberg/zeta/actions) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A composable, type-safe, async-first validation framework for .NET inspired by [Zod](https://zod.dev/).

## Basic example
```csharp
var UserSchema = Z.Object<User>()
    .Field(u => u.Email, s => s.Email())
    .Field(u => u.Age, s => s.Min(18));

var result = await UserSchema.ValidateAsync(user);

if(!result.IsSuccess)
{
    foreach(var error in result.Errors)
    {
        Console.WriteLine($"{error.Path}: {error.Message}");
    }
}
```

## Async example

Copy-paste example for context-aware validation where context is built from async services:

```csharp
using Microsoft.Extensions.DependencyInjection;

public sealed record SignupRequest(string Email, string ReferralCode);
public sealed record SignupContext(bool EmailAvailable, bool ReferralValid);

public interface IAccountService
{
    Task<bool> IsEmailAvailableAsync(string email, CancellationToken ct);
}

public interface IReferralService
{
    Task<bool> IsValidCodeAsync(string code, CancellationToken ct);
}

var signupSchema = Z.Object<SignupRequest>()
    .Field(x => x.Email, s => s.Email())
    .Field(x => x.ReferralCode, s => s.MinLength(6))
    .Using<SignupContext>(async (input, sp, ct) =>
    {
        var accounts = sp.GetRequiredService<IAccountService>();
        var referrals = sp.GetRequiredService<IReferralService>();

        var emailAvailable = await accounts.IsEmailAvailableAsync(input.Email, ct);
        var referralValid = await referrals.IsValidCodeAsync(input.ReferralCode, ct);

        return new SignupContext(emailAvailable, referralValid);
    })
    .Refine((_, ctx) => ctx.EmailAvailable, "Email is already in use")
    .Refine((_, ctx) => ctx.ReferralValid, "Referral code is invalid");

// Use IZetaValidator so IServiceProvider is available to the factory
Result<SignupRequest> result = await zetaValidator.ValidateAsync(request, signupSchema, ct);
```

## Features

- **Schema-first** - Define validation as reusable schema objects
- **Async by default** - Every rule can be async, no separate sync/async paths
- **Composable** - Schemas are values that can be reused and combined
- **Path-aware errors** - Errors include location (`user.address.street`, `items[0]`)
- **ASP.NET Core native** - First-class support for Minimal APIs and Controllers

## Installation

```bash
dotnet add package Zeta
dotnet add package Zeta.AspNetCore  # For ASP.NET Core integration
```

## Schema Types

### String

```csharp
Z.String()
    .MinLength(3)
    .MaxLength(100)
    .Length(10)           // Exact length
    .Email()
    .Uuid()               // UUID/GUID format
    .Url()                // HTTP/HTTPS URLs
    .Uri()                // Any valid URI
    .Alphanumeric()       // Letters and numbers only
    .StartsWith("prefix")
    .EndsWith("suffix")
    .Contains("substring")
    .Regex(@"^[A-Z]")
    .NotEmpty()
    .Refine(s => s.StartsWith("A"), "Must start with A")
```

### Numeric Types

```csharp
Z.Int()
    .Min(0)
    .Max(100)
    .Refine(n => n % 2 == 0, "Must be even")

Z.Double()
    .Min(0.0)
    .Max(100.0)
    .Positive()
    .Negative()
    .Finite()

Z.Decimal()
    .Min(0m)
    .Max(1000m)
    .Positive()
    .Precision(2)       // Max 2 decimal places
    .MultipleOf(0.25m)  // Must be multiple of step
```

### Date and Time

```csharp
Z.DateTime()
    .Min(minDate)
    .Max(maxDate)
    .Past()             // Must be in the past
    .Future()           // Must be in the future
    .Between(min, max)
    .Weekday()          // Monday-Friday only
    .Weekend()          // Saturday-Sunday only
    .WithinDays(7)      // Within N days from now
    .MinAge(18)         // For birthdate validation
    .MaxAge(65)

Z.DateOnly()
    .Min(minDate).Max(maxDate).Past().Future()
    .Between(min, max).Weekday().Weekend()
    .MinAge(18).MaxAge(65)

Z.TimeOnly()
    .Min(minTime).Max(maxTime).Between(min, max)
    .BusinessHours()              // 9 AM - 5 PM (default)
    .BusinessHours(start, end)    // Custom hours
    .Morning()                    // 6 AM - 12 PM
    .Afternoon()                  // 12 PM - 6 PM
    .Evening()                    // 6 PM - 12 AM
```

### Other Types

```csharp
Z.Guid()
    .NotEmpty()         // Not Guid.Empty
    .Version(4)         // Specific UUID version (1, 2, 3, 4, or 5)

Z.Bool()
    .IsTrue()           // Must be true (e.g., terms accepted)
    .IsFalse()          // Must be false
```

### Nullable (Optional Fields)

All schemas are required by default. Use `.Nullable()` to allow null values:

```csharp
Z.String().Nullable()           // Allows null strings
Z.Int().Nullable()              // Allows null ints
Z.Object<Address>().Nullable()  // Allows null objects
```

**Nullable value type fields** (`int?`, `double?`, etc.) are handled automatically in object schemas — null values skip validation, non-null values are validated. No need to call `.Nullable()`:

```csharp
public record User(string Name, int? Age, decimal? Balance, string? Bio);

Z.Object<User>()
    .Field(u => u.Name, s => s.MinLength(2))
    .Field(u => u.Age, s => s.Min(0).Max(120))          // int? — null skips validation
    .Field(u => u.Balance, s => s.Positive().Precision(2)) // decimal? — null skips validation
    .Field(u => u.Bio, s => s.MaxLength(500).Nullable())   // string? — call .Nullable() to allow null
```

For nullable reference types (`string?`), call `.Nullable()` on the schema if null should be a valid value.

### Object

Define field schemas inline with builder functions:

```csharp
Z.Object<User>()
    .Field(u => u.Name, s => s.MinLength(2))
    .Field(u => u.Email, s => s.Email().MinLength(5))
    .Field(u => u.Age, s => s.Min(18).Max(100))
    .Field(u => u.Price, s => s.Positive().Precision(2))
    .Refine(u => u.Password != u.Email, "Password cannot be email");

// Supported for: string, int, double, decimal, bool, Guid, DateTime, DateOnly, TimeOnly
```

**For Composability** - Extract reusable schemas when needed across multiple objects:

```csharp
var AddressSchema = Z.Object<Address>()
    .Field(a => a.Street, s => s.MinLength(3))
    .Field(a => a.ZipCode, s => s.Regex(@"^\d{5}$"));

Z.Object<User>()
    .Field(u => u.Name, s => s.MinLength(2))
    .Field(u => u.Address, AddressSchema);  // Reuse nested schema
```

### Collections

Validate arrays and lists with `.Each()` for element validation:

```csharp
// Simple element validation
Z.Collection<string>()
    .Each(s => s.Email())          // Validate each element
    .MinLength(1)                  // Collection-level validation
    .MaxLength(10)

Z.Collection<int>()
    .Each(n => n.Min(0).Max(100))
    .NotEmpty()

// In object schemas with fluent builders
Z.Object<User>()
    .Field(u => u.Tags, tags => tags
        .Each(s => s.MinLength(3).MaxLength(50))
        .MinLength(1).MaxLength(10));

// Complex nested objects - pass pre-built schema
var orderItemSchema = Z.Object<OrderItem>()
    .Field(i => i.ProductId, s => s)
    .Field(i => i.Quantity, s => s.Min(1));

Z.Collection(orderItemSchema)
    .MinLength(1);

// Errors include index path: "tags[0]", "items[2].quantity"
```

See the [Collections guide](docs/Collections.md) for advanced patterns including context-aware validation.

### Conditional Validation

Use `.If()` for guard-style conditional validation on any schema type:

```csharp
// Object schemas — conditional field validation
Z.Object<Order>()
    .Field(o => o.PaymentMethod, s => s.NotEmpty())
    .If(o => o.PaymentMethod == "card", s => s
        .Field(o => o.CardNumber, n => n.MinLength(16)))
    .If(o => o.PaymentMethod == "bank", s => s
        .Field(o => o.BankAccount, n => n.MinLength(10)));

// Value schemas — apply rules only when predicate matches
Z.Int()
    .If(v => v >= 18, s => s.Max(65));

// Multiple guards and nesting
Z.Int()
    .If(v => v >= 0, s => s
        .If(v => v >= 18, inner => inner.Max(100)));

// Context-aware: value+context predicates
Z.String()
    .Using<SecurityContext>()
    .If((v, ctx) => ctx.RequireStrongPassword, s => s.MinLength(12));
```

### Polymorphic Validation

Use branch schemas with `.If(predicate, schema)` or `.WhenType<TDerived>(...)`:

```csharp
var dogSchema = Z.Object<Dog>()
    .Field(x => x.BarkVolume, x => x.Min(0).Max(100));

var schema = Z.Object<IAnimal>()
    .If(x => x is Dog, dogSchema)
    .WhenType<Cat>(cat => cat
        .Field(x => x.ClawSharpness, x => x.Min(1).Max(10)));

// Explicit type assertion is still available
Z.Object<IAnimal>().As<Dog>();  // Fails with type_mismatch if not a Dog
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
builder.Services.AddZeta();
```

### Minimal APIs

```csharp
var UserSchema = Z.Object<User>()
    .Field(u => u.Email, s => s.Email())
    .Field(u => u.Name, s => s.MinLength(3));

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
        .Field(u => u.Name, s => s.MinLength(3))
        .Field(u => u.Email, s => s.Email());

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

Configure execution context using a builder function:

```csharp
var result = await _validator.ValidateAsync(
    user,
    UserSchema,
    b => b.WithCancellation(ct).WithTimeProvider(fakeTimeProvider));
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

### Reusable Custom Logic

```csharp
public static class StringSchemaExtensions
{
    public static StringContextlessSchema StartsWithUpper(this StringContextlessSchema schema)
        => schema.Refine(
            value => value.Length > 0 && char.IsUpper(value[0]),
            "Must start with uppercase",
            "starts_upper");
}

// Usage
Z.String().StartsWithUpper();
```

### Async Refinement

For async validation with context:

```csharp
Z.String()
    .Email()
    .Using<UserContext>()
    .RefineAsync(
        async (email, ctx, ct) => !await _repo.EmailExistsAsync(email, ct),
        message: "Email already taken",
        code: "email_exists"
    )
```

See the [Custom Rules guide](docs/CustomRules.md) for context-aware rules and advanced patterns.

## Validation Context

For async data loading before validation (e.g., checking database):

```csharp
// Define context
public record UserContext(bool EmailExists);

var UserSchema = Z.Object<User>()
    .Field(u => u.Name, s => s.MinLength(3))
    .Using<UserContext>(async (input, sp, ct) =>
    {
        var repo = sp.GetRequiredService<IUserRepository>();
        var exists = await repo.EmailExistsAsync(input.Email, ct);
        return new UserContext(EmailExists: exists);
    })
    .Field(u => u.Email, s => s.Email())  // Fluent builders still work
    // For context-aware validation, use pre-built schemas
    .Field(u => u.Username,
        Z.String()
            .MinLength(3)
            .Using<UserContext>()
            .RefineAsync(async (username, ctx, ct) =>
                !await ctx.Repo.UsernameExistsAsync(username, ct),
                "Username already taken"));
```

See the [Validation Context guide](docs/ValidationContext.md) for more details.

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
| `null_value` | Value is null but schema is non-nullable |
| `min_length` / `max_length` | Length constraints |
| `length` | Not exact length |
| `min_value` / `max_value` | Value constraints |
| `email` / `uuid` / `url` / `uri` | Format validation |
| `alphanumeric` | Contains non-alphanumeric chars |
| `starts_with` / `ends_with` / `contains` | String content |
| `regex` | Pattern mismatch |
| `precision` / `multiple_of` | Numeric constraints |
| `positive` / `negative` / `finite` | Number signs |
| `past` / `future` / `between` / `within_days` | Date constraints |
| `weekday` / `weekend` | Day of week |
| `min_age` / `max_age` | Age validation |
| `business_hours` / `morning` / `afternoon` / `evening` | Time constraints |
| `not_empty` / `version` | GUID validation |
| `is_true` / `is_false` | Boolean constraints |
| `null_value` | Value is null but schema is non-nullable |
| `type_mismatch` | Type assertion failed (`.As<T>()`) |
| `custom_error` | Custom refinement failed |

## Benchmarks

Comparing Zeta against FluentValidation and DataAnnotations on .NET 10 (Apple M2 Pro).

| Method | Mean | Allocated |
|--------|-----:|----------:|
| FluentValidation | 129.1 ns | 600 B |
| FluentValidation (Async) | 229.3 ns | 672 B |
| **Zeta** | **293.2 ns** | **152 B** |
| Zeta (Invalid) | 398.6 ns | 904 B |
| DataAnnotations | 617.8 ns | 1,848 B |
| DataAnnotations (Invalid) | 974.9 ns | 2,672 B |
| FluentValidation (Invalid) | 1,920.5 ns | 7,728 B |
| FluentValidation (Invalid Async) | 2,095.8 ns | 7,800 B |

**Key findings:**
- Allocates **75% less memory** than FluentValidation on valid input (152 B vs 600 B)
- Allocates **8.5x less memory** than FluentValidation on invalid input (904 B vs 7,728 B)
- **4.8x faster** than FluentValidation when validation fails (398 ns vs 1,920 ns)
- **2.4x faster** than DataAnnotations when validation fails (398 ns vs 974 ns)

Run benchmarks:
```bash
dotnet run --project benchmarks/Zeta.Benchmarks -c Release
```

## Documentation

- [Collections](docs/Collections.md) - Arrays, lists, and element validation patterns
- [Fluent Field Builders](docs/FluentFieldBuilders.md) - Inline schema definitions for object fields
- [Validation Context](docs/ValidationContext.md) - Async data loading and context-aware schemas
- [Custom Rules](docs/CustomRules.md) - Creating reusable validation rules
- [Testing](docs/Testing.md) - Testing strategies and TimeProvider support
- [Mediator Integration](docs/Mediator.md) - Using Zeta with MediatR pipelines
- [Changelog](CHANGELOG.md) - Release history and version changes

## License

MIT
