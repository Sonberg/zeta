# Zeta

A composable, type-safe, async-first validation framework for .NET inspired by [Zod](https://zod.dev/).

## Features

- **Schema-first** — Define validation as reusable schema objects
- **Async by default** — Every rule can be async, no separate sync/async paths
- **Result pattern** — No exceptions for validation failures
- **Composable** — Schemas are values that can be reused and combined
- **Path-aware errors** — Errors include location (`user.address.street`, `items[0]`)
- **ASP.NET Core native** — First-class support for Minimal APIs and Controllers
- 
## Mental Model

- A **Schema** describes how to validate a value
- Schemas are immutable and composable
- Validation returns a `Result<T>`, never throws
- Context is opt-in and never leaks into schemas unless explicitly enabled
- Errors are data, not exceptions

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

### DateTime

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
```

### DateOnly

```csharp
Z.DateOnly()
    .Min(minDate)
    .Max(maxDate)
    .Past()
    .Future()
    .Between(min, max)
    .Weekday()
    .Weekend()
    .MinAge(18)
    .MaxAge(65)
```

### TimeOnly

```csharp
Z.TimeOnly()
    .Min(minTime)
    .Max(maxTime)
    .Between(min, max)
    .BusinessHours()              // 9 AM - 5 PM (default)
    .BusinessHours(start, end)    // Custom hours
    .Morning()                    // Before noon
    .Afternoon()                  // Noon to 6 PM
    .Evening()                    // After 6 PM
```

### Guid

```csharp
Z.Guid()
    .NotEmpty()         // Not Guid.Empty
    .Version(4)         // Specific UUID version (1-5)
```

### Bool

```csharp
Z.Bool()
    .IsTrue()           // Must be true (e.g., terms accepted)
    .IsFalse()          // Must be false
```

### Nullable (Optional Fields)

All schemas are required by default. Use `.Nullable()` to make fields optional:

```csharp
Z.String().Nullable()           // Allows null strings
Z.Int().Nullable()              // Allows null ints (int?)
Z.DateTime().Nullable()         // Allows null DateTime (DateTime?)
Z.Object<Address>().Nullable()  // Allows null objects

// In object schemas
Z.Object<User>()
    .Field(u => u.MiddleName, Z.String().Nullable())  // Optional field
    .Field(u => u.Age, Z.Int().Min(0).Nullable())     // Optional with validation
```

### Object

```csharp
Z.Object<User>()
    .Field(u => u.Name, Z.String().MinLength(2))
    .Field(u => u.Email, Z.String().Email())
    .Field(u => u.Address, AddressSchema)  // Nested schemas
    .Refine(u => u.Password != u.Email, "Password cannot be email")
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

### Conditional Validation

Use `.When()` for dependent field validation:

```csharp
Z.Object<Order>()
    .Field(o => o.PaymentMethod, Z.String())
    .Field(o => o.CardNumber, Z.String().Nullable())
    .Field(o => o.BankAccount, Z.String().Nullable())
    .When(
        o => o.PaymentMethod == "card",
        then: c => c.Require(o => o.CardNumber),    // CardNumber required when paying by card
        @else: c => c.Require(o => o.BankAccount)  // BankAccount required otherwise
    );

// Multiple conditions
Z.Object<User>()
    .Field(u => u.IsCompany, Z.Bool())
    .Field(u => u.CompanyName, Z.String().Nullable())
    .Field(u => u.VatNumber, Z.String().Nullable())
    .When(
        u => u.IsCompany,
        then: c => c
            .Require(u => u.CompanyName)
            .Require(u => u.VatNumber)
    );
```

#### Inline Field Validation with Select

> Select lets you temporarily “zoom in” on one or more fields and apply additional rules without redefining the field schema.

Use `.Select()` for inline schema building within conditionals:

```csharp
Z.Object<User>()
    .Field(u => u.Password, Z.String().Nullable())
    .Field(u => u.RequiresStrongPassword, Z.Bool())
    .When(
        u => u.RequiresStrongPassword,
        then: c => c.Select(u => u.Password, s => s.MinLength(12).MaxLength(100))
    );

// Supported types: string, int, double, decimal (and their nullable variants)
Z.Object<Product>()
    .Field(p => p.Price, Z.Decimal().Nullable())
    .Field(p => p.Quantity, Z.Int().Nullable())
    .When(
        p => p.IsActive,
        then: c => c
            .Select(p => p.Price, s => s.Min(0.01m).Max(10000m))
            .Select(p => p.Quantity, s => s.Min(1).Max(1000))
    );

// Context-aware conditional with Select
Z.Object<User>()
    .WithContext<User, UserContext>()
    .When(
        (_, ctx) => ctx.RequireStrongPassword,
        then: c => c.Select(u => u.Password, s => s.MinLength(12).MaxLength(100))
    );
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

Schemas are always created contextless. Use `.WithContext<TContext>()` to promote a schema when you need access to validation context:

```csharp
// Promote schema to context-aware when needed
var EmailSchema = Z.String()
    .Email()
    .WithContext<UserContext>()
    .Refine((email, ctx) => !ctx.EmailExists, "Email already taken");

// For ObjectSchema, you can add .Field() before or after .WithContext()
var UserSchema = Z.Object<User>()
    .Field(u => u.Name, Z.String().MinLength(3))
    .WithContext<User, UserContext>()
    .Field(u => u.Email, EmailSchema)
    .Refine((user, ctx) => !ctx.IsMaintenanceMode, "Registration disabled");
```

You can also use context-free `Refine()` on a promoted schema:

```csharp
Z.String()
    .WithContext<UserContext>()
    .Refine(val => val.Length > 3, "Too short")                    // No context
    .Refine((val, ctx) => val != ctx.BannedEmail, "Email banned"); // With context
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

For contextless schemas, implement `IValidationRule<T>`:

```csharp
public sealed class StartsWithUpperRule : IValidationRule<string>
{
    public ValidationError? Validate(string value, ValidationExecutionContext execution)
    {
        return char.IsUpper(value[0])
            ? null
            : new ValidationError(execution.Path, "starts_upper", "Must start with uppercase");
    }
}

// Usage
Z.String().Use(new StartsWithUpperRule())
```

For context-aware schemas, implement `IValidationRule<T, TContext>`:

```csharp
public sealed class UniqueEmailRule<TContext> : IValidationRule<string, TContext>
    where TContext : IEmailContext
{
    public ValidationError? Validate(string value, ValidationContext<TContext> ctx)
    {
        return ctx.Data.EmailExists
            ? new ValidationError(ctx.Execution.Path, "email_taken", "Email already taken")
            : null;
    }
}

// Usage
Z.String()
    .WithContext<UserContext>()
    .Use(new UniqueEmailRule<UserContext>())
```

## Testing

Zeta supports `TimeProvider` for testing time-based validations like `Past()`, `Future()`, `MinAge()`, and `WithinDays()`.

### Unit Testing Schemas

```csharp
using Microsoft.Extensions.Time.Testing;
using Zeta.Core;

[Fact]
public async Task MinAge_WithFakeTime_ValidatesCorrectly()
{
    // Arrange - "now" is June 15, 2024
    var fakeTime = new FakeTimeProvider(
        new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
    var context = new ValidationExecutionContext(timeProvider: fakeTime);
    var schema = Z.DateTime().MinAge(18);

    // Someone born June 16, 2006 is not yet 18 on June 15, 2024
    var birthDate = new DateTime(2006, 6, 16, 0, 0, 0, DateTimeKind.Utc);

    // Act
    var result = await schema.ValidateAsync(birthDate, context);

    // Assert
    Assert.True(result.IsFailure);
    Assert.Contains(result.Errors, e => e.Code == "min_age");

    // Advance time by 1 day - now they're 18
    fakeTime.Advance(TimeSpan.FromDays(1));
    var newContext = new ValidationExecutionContext(timeProvider: fakeTime);

    var result2 = await schema.ValidateAsync(birthDate, newContext);
    Assert.True(result2.IsSuccess);
}
```

### Testing with IZetaValidator

Register `TimeProvider` in the service collection to control time in controller tests:

```csharp
[Fact]
public async Task CreateUser_UnderageUser_ReturnsValidationError()
{
    // Arrange
    var fakeTime = new FakeTimeProvider(
        new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));

    var services = new ServiceCollection();
    services.AddSingleton<TimeProvider>(fakeTime);
    services.AddScoped<IZetaValidator, ZetaValidator>();
    var serviceProvider = services.BuildServiceProvider();

    var validator = serviceProvider.GetRequiredService<IZetaValidator>();
    var schema = Z.Object<UserRequest>()
        .Field(x => x.Email, Z.String().Email())
        .Field(x => x.BirthDate, Z.DateTime().MinAge(18));

    var request = new UserRequest("test@example.com", new DateTime(2010, 1, 1));

    // Act
    var result = await validator.ValidateAsync(request, schema);

    // Assert
    Assert.True(result.IsFailure);
    Assert.Contains(result.Errors, e => e.Code == "min_age");
}
```

### Integration Testing with WebApplicationFactory

```csharp
public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task CreateBooking_FutureDate_Succeeds()
    {
        var fakeTime = new FakeTimeProvider(
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<TimeProvider>(fakeTime);
                });
            });

        var client = factory.CreateClient();

        // Date is in the "future" relative to fake time
        var response = await client.PostAsJsonAsync("/bookings", new
        {
            date = "2024-06-15"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

The `FakeTimeProvider` class is from the `Microsoft.Extensions.TimeProvider.Testing` package.

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
| `length` | Not exact length |
| `min_value` | Below minimum value |
| `max_value` | Above maximum value |
| `min_date` | Before minimum date |
| `max_date` | After maximum date |
| `min_time` | Before minimum time |
| `max_time` | After maximum time |
| `email` | Invalid email format |
| `uuid` | Invalid UUID format |
| `url` | Invalid URL format |
| `uri` | Invalid URI format |
| `alphanumeric` | Contains non-alphanumeric chars |
| `starts_with` | Missing required prefix |
| `ends_with` | Missing required suffix |
| `contains` | Missing required substring |
| `regex` | Pattern mismatch |
| `precision` | Too many decimal places |
| `multiple_of` | Not a multiple of step value |
| `positive` | Must be positive |
| `negative` | Must be negative |
| `finite` | Must be finite number |
| `past` | Must be in the past |
| `future` | Must be in the future |
| `between` | Outside allowed range |
| `within_days` | Outside allowed day range |
| `weekday` | Must be a weekday |
| `weekend` | Must be a weekend |
| `min_age` | Below minimum age |
| `max_age` | Above maximum age |
| `business_hours` | Outside business hours |
| `morning` | Not in morning hours |
| `afternoon` | Not in afternoon hours |
| `evening` | Not in evening hours |
| `not_empty` | GUID is empty |
| `version` | Invalid UUID version |
| `is_true` | Must be true |
| `is_false` | Must be false |
| `custom_error` | Custom refinement failed |

## Benchmarks

Comparing Zeta against FluentValidation and DataAnnotations on .NET 9 (Apple M2 Pro).

### Valid Input

| Method | Mean | Allocated |
|--------|-----:|----------:|
| FluentValidation | 151 ns | 600 B |
| FluentValidation (Async) | 270 ns | 672 B |
| **Zeta** | **308 ns** | **120 B** |
| DataAnnotations | 674 ns | 1,880 B |

### Invalid Input (with errors)

| Method | Mean | Allocated |
|--------|-----:|----------:|
| **Zeta** | **461 ns** | **752 B** |
| DataAnnotations | 1,114 ns | 2,704 B |
| FluentValidation | 2,240 ns | 7,920 B |
| FluentValidation (Async) | 2,434 ns | 7,992 B |

**Key findings:**
- Zeta uses **5x less memory** than FluentValidation for valid input (120 B vs 600 B)
- Zeta is **5x faster** than FluentValidation when validation fails
- Zeta uses **10x less memory** than FluentValidation for invalid input (752 B vs 7,920 B)
- Zeta is **2x faster** than DataAnnotations in all scenarios
- For valid input, FluentValidation sync is fastest, but Zeta has minimal allocations

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
- Better IDE support for schema definitions (source generators?)
- More ASP.NET Core integration features (filters, model binders)
- Precompiled schema definitions for common types
- Localization support for error messages
- Integration with OpenAPI/Swagger for schema generation
- Support for other .NET platforms (e.g., Xamarin, MAUI)