# Zeta

A composable, type-safe, async-first validation framework for .NET inspired by [Zod](https://zod.dev/).

```csharp
var UserSchema = Z.Object<User>()
    .Field(u => u.Email, Z.String().Email())
    .Field(u => u.Age, Z.Int().Min(18));

var result = await UserSchema.ValidateAsync(user);

result.Match(
    success: user => Console.WriteLine($"Valid: {user.Email}"),
    failure: errors => errors.ForEach(e => Console.WriteLine($"{e.Path}: {e.Message}"))
);
```

## Features

- **Schema-first** - Define validation as reusable schema objects
- **Async by default** - Every rule can be async, no separate sync/async paths
- **Result pattern** - No exceptions for validation failures
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
    .Morning().Afternoon().Evening()
```

### Other Types

```csharp
Z.Guid()
    .NotEmpty()         // Not Guid.Empty
    .Version(4)         // Specific UUID version (1-5)

Z.Bool()
    .IsTrue()           // Must be true (e.g., terms accepted)
    .IsFalse()          // Must be false
```

### Nullable (Optional Fields)

All schemas are required by default. Use `.Nullable()` to make fields optional:

```csharp
Z.String().Nullable()           // Allows null strings
Z.Int().Nullable()              // Allows null ints (int?)
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

### Collections

```csharp
Z.Array(Z.String().Email())
    .MinLength(1)
    .MaxLength(10)
    .NotEmpty()

Z.List(Z.Int().Min(0))
    .MinLength(1)
    .MaxLength(100)
    .NotEmpty()

// Errors include index path: "emails[0]", "items[2].name"
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
```

Use `.Select()` for inline schema building within conditionals:

```csharp
Z.Object<User>()
    .Field(u => u.Password, Z.String().Nullable())
    .Field(u => u.RequiresStrongPassword, Z.Bool())
    .When(
        u => u.RequiresStrongPassword,
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
builder.Services.AddZeta();
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

See the [Custom Rules guide](docs/CustomRules.md) for context-aware rules and advanced patterns.

## Validation Context

For async data loading before validation (e.g., checking database):

```csharp
// Define context
public record UserContext(bool EmailExists);

// Factory to load context
public class UserContextFactory : IValidationContextFactory<User, UserContext>
{
    private readonly IUserRepository _repo;

    public UserContextFactory(IUserRepository repo) => _repo = repo;

    public async Task<UserContext> CreateAsync(User input, IServiceProvider services, CancellationToken ct)
    {
        return new UserContext(EmailExists: await _repo.EmailExistsAsync(input.Email, ct));
    }
}

// Use in schema with .WithContext()
var UserSchema = Z.Object<User>()
    .Field(u => u.Name, Z.String().MinLength(3))
    .WithContext<User, UserContext>()
    .Field(u => u.Email,
        Z.String()
            .Email()
            .WithContext<UserContext>()
            .Refine((email, ctx) => !ctx.EmailExists, "Email already taken"));

// Register
builder.Services.AddZeta(typeof(Program).Assembly);
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
| `required` | Value is null/missing |
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
| `custom_error` | Custom refinement failed |

## Benchmarks

Comparing Zeta against FluentValidation and DataAnnotations on .NET 9 (Apple M2 Pro).

| Method | Mean | Allocated |
|--------|-----:|----------:|
| FluentValidation | 150 ns | 600 B |
| FluentValidation (Async) | 270 ns | 672 B |
| **Zeta** | **374 ns** | **416 B** |
| Zeta (Invalid) | 555 ns | 1,144 B |
| DataAnnotations | 733 ns | 1,880 B |
| DataAnnotations (Invalid) | 1,105 ns | 2,704 B |
| FluentValidation (Invalid) | 2,247 ns | 7,920 B |

**Key findings:**
- Allocates **~40% less memory** than FluentValidation on valid input
- Allocates **~7x less memory** than FluentValidation on invalid input
- **4x faster** than FluentValidation when validation fails
- **2x faster** than DataAnnotations when validation fails

Run benchmarks:
```bash
dotnet run --project benchmarks/Zeta.Benchmarks -c Release
```

## Documentation

- [Validation Context](docs/ValidationContext.md) - Async data loading and context-aware schemas
- [Custom Rules](docs/CustomRules.md) - Creating reusable validation rules
- [Testing](docs/Testing.md) - Testing strategies and TimeProvider support
- [Mediator Integration](docs/Mediator.md) - Using Zeta with MediatR pipelines

## License

MIT
