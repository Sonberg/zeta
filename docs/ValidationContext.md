# Validation Context

This guide covers Zeta's validation context system for async data loading and context-aware validation rules.

---

## When to Use Context

Use validation context when you need to:

- Check data against a database (e.g., "email already exists")
- Load configuration or feature flags before validation
- Access user permissions or roles during validation
- Validate against external services

---

## Define a Context

A context is any class or record that holds data needed during validation:

```csharp
public record UserContext(
    bool EmailExists,
    bool IsMaintenanceMode,
    List<string> BannedDomains
);
```

---

## Create a Context Factory

Implement `IValidationContextFactory<TInput, TContext>` to load context data before validation runs:

```csharp
public class UserContextFactory : IValidationContextFactory<User, UserContext>
{
    private readonly IUserRepository _repo;
    private readonly IFeatureFlags _features;

    public UserContextFactory(IUserRepository repo, IFeatureFlags features)
    {
        _repo = repo;
        _features = features;
    }

    public async Task<UserContext> CreateAsync(
        User input,
        IServiceProvider services,
        CancellationToken ct)
    {
        var emailExists = await _repo.EmailExistsAsync(input.Email, ct);
        var maintenanceMode = await _features.IsEnabledAsync("maintenance", ct);
        var bannedDomains = await _repo.GetBannedDomainsAsync(ct);

        return new UserContext(emailExists, maintenanceMode, bannedDomains);
    }
}
```

Register factories via assembly scanning:

```csharp
builder.Services.AddZeta(typeof(Program).Assembly);
```

---

## Promote Schemas to Context-Aware

Schemas start contextless. Use `.WithContext<TContext>()` to enable context access:

```csharp
// Basic string schema
var EmailSchema = Z.String().Email();

// Promoted to context-aware
var ContextAwareEmailSchema = Z.String()
    .Email()
    .WithContext<UserContext>()
    .Refine((email, ctx) => !ctx.EmailExists, "Email already taken");
```

Rules defined before `.WithContext()` are preserved:

```csharp
Z.String()
    .Email()               // This rule is kept
    .MinLength(5)          // This rule is kept
    .WithContext<UserContext>()
    .Refine((email, ctx) => !ctx.BannedDomains.Any(d => email.EndsWith(d)),
        "Domain not allowed");
```

---

## Context-Aware Object Schemas

For objects, use `.WithContext<T, TContext>()`:

```csharp
var UserSchema = Z.Object<User>()
    .Field(u => u.Name, Z.String().MinLength(3))
    .WithContext<User, UserContext>()
    .Field(u => u.Email,
        Z.String()
            .Email()
            .WithContext<UserContext>()
            .Refine((email, ctx) => !ctx.EmailExists, "Email already taken"))
    .Refine((user, ctx) => !ctx.IsMaintenanceMode, "Registration disabled");
```

You can add fields before or after `.WithContext()`:

```csharp
// Fields before WithContext use contextless schemas
Z.Object<User>()
    .Field(u => u.Name, Z.String().MinLength(3))  // Contextless
    .WithContext<User, UserContext>()
    .Field(u => u.Email, emailSchemaWithContext)  // Context-aware
```

---

## Mixed Context and Contextless Rules

After promoting a schema, you can use both context-free and context-aware refinements:

```csharp
Z.String()
    .WithContext<UserContext>()
    .Refine(val => val.Length > 3, "Too short")                    // No context
    .Refine((val, ctx) => val != ctx.BannedEmail, "Email banned"); // With context
```

---

## Context-Aware Conditionals

Use context in `.When()` conditions:

```csharp
Z.Object<User>()
    .Field(u => u.Password, Z.String().MinLength(8))
    .WithContext<User, SecurityContext>()
    .When(
        (user, ctx) => ctx.RequireStrongPassword,
        then: c => c.Select(u => u.Password, s => s.MinLength(12).MaxLength(100))
    );
```

---

## Manual Validation with Context

For controllers or handlers that need manual validation:

```csharp
public class UsersController : ControllerBase
{
    private readonly IZetaValidator _validator;
    private readonly IUserRepository _repo;

    private static readonly ISchema<User, UserContext> UserSchema =
        Z.Object<User>()
            .WithContext<User, UserContext>()
            .Field(u => u.Email,
                Z.String()
                    .Email()
                    .WithContext<UserContext>()
                    .Refine((e, ctx) => !ctx.EmailExists, "Email taken"));

    [HttpPost]
    public async Task<IActionResult> Create(User user, CancellationToken ct)
    {
        var context = new UserContext(
            EmailExists: await _repo.EmailExistsAsync(user.Email, ct)
        );

        var result = await _validator.ValidateAsync(user, UserSchema, context, ct);

        return result.ToActionResult(valid => Ok(new { Message = "Created" }));
    }
}
```

---

## Context Factory Best Practices

### Handle Soft Failures

Factory exceptions propagate as HTTP 500. For expected failures, return a context that causes validation to fail:

```csharp
public async Task<UserContext> CreateAsync(User input, IServiceProvider services, CancellationToken ct)
{
    try
    {
        var emailExists = await _repo.EmailExistsAsync(input.Email, ct);
        return new UserContext(emailExists, ServiceAvailable: true);
    }
    catch (ServiceUnavailableException)
    {
        // Return context that will fail validation gracefully
        return new UserContext(EmailExists: false, ServiceAvailable: false);
    }
}

// Schema checks service availability
.Refine((_, ctx) => ctx.ServiceAvailable, "Service temporarily unavailable")
```

### Keep Factories Focused

Load only what's needed for validation:

```csharp
// Good - loads specific data needed for validation
public async Task<UserContext> CreateAsync(User input, ...)
{
    return new UserContext(
        EmailExists: await _repo.EmailExistsAsync(input.Email, ct)
    );
}

// Avoid - loading unnecessary data
public async Task<UserContext> CreateAsync(User input, ...)
{
    var user = await _repo.GetFullUserProfileAsync(input.Email, ct);
    var orders = await _orderRepo.GetAllOrdersAsync(user.Id, ct);
    // ...
}
```

### Cache When Appropriate

For expensive lookups used across multiple fields:

```csharp
public record ProductContext(
    Dictionary<string, bool> CategoryExists,
    Dictionary<string, decimal> ProductPrices
);

public async Task<ProductContext> CreateAsync(Order input, ...)
{
    var categoryIds = input.Items.Select(i => i.CategoryId).Distinct();
    var productIds = input.Items.Select(i => i.ProductId).Distinct();

    var categories = await _repo.CategoriesExistAsync(categoryIds, ct);
    var prices = await _repo.GetPricesAsync(productIds, ct);

    return new ProductContext(categories, prices);
}
```
