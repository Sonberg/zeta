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

## Create Context Inline

Use `.Using<TContext>(factory)` to load context data before validation runs:

```csharp
var schema = Z.Schema<User>()
    .Property(u => u.Email, s => s.Email())
    .Using<UserContext>(async (input, sp, ct) =>
    {
        var repo = sp.GetRequiredService<IUserRepository>();
        var features = sp.GetRequiredService<IFeatureFlags>();

        var emailExists = await repo.EmailExistsAsync(input.Email, ct);
        var maintenanceMode = await features.IsEnabledAsync("maintenance", ct);
        var bannedDomains = await repo.GetBannedDomainsAsync(ct);

        return new UserContext(emailExists, maintenanceMode, bannedDomains);
    });
```

---

## Promote Schemas to Context-Aware

Schemas start contextless. Use `.Using<TContext>()` to enable context access:

```csharp
// Basic string schema
var EmailSchema = Z.String().Email();

// Promoted to context-aware
var ContextAwareEmailSchema = Z.String()
    .Email()
    .Using<UserContext>()
    .Refine((email, ctx) => !ctx.EmailExists, "Email already taken");
```

Context-aware schemas are also assignable to `ISchema<T>`. They still resolve context through their configured factory when validated in a contextless flow that includes `IServiceProvider`.

Rules defined before `.Using()` are preserved:

```csharp
Z.String()
    .Email()               // This rule is kept
    .MinLength(5)          // This rule is kept
    .Using<UserContext>()
    .Refine((email, ctx) => !ctx.BannedDomains.Any(d => email.EndsWith(d)),
        "Domain not allowed");
```

---

## Context-Aware Object Schemas

For objects, use `.Using<TContext>()`:

```csharp
var UserSchema = Z.Schema<User>()
    .Property(u => u.Name, Z.String().MinLength(3))
    .Using<UserContext>()
    .Property(u => u.Email,
        Z.String()
            .Email()
            .Using<UserContext>()
            .Refine((email, ctx) => !ctx.EmailExists, "Email already taken"))
    .Refine((user, ctx) => !ctx.IsMaintenanceMode, "Registration disabled");
```

If a context-aware object rule should report on a specific property, use `RefineAt(...)`:

```csharp
Z.Schema<User>()
    .Using<UserContext>()
    .RefineAt(
        u => u.Email,
        (u, ctx) => !ctx.BannedDomains.Any(d => u.Email.EndsWith(d)),
        "Domain not allowed");
```

You can add fields before or after `.Using()`:

```csharp
// Fields before Using use contextless schemas
Z.Schema<User>()
    .Property(u => u.Name, Z.String().MinLength(3))  // Contextless
    .Using<UserContext>()
    .Property(u => u.Email, emailSchemaWithContext)  // Context-aware
```

---

## Mixed Context and Contextless Rules

After promoting a schema, you can use both context-free and context-aware refinements:

```csharp
Z.String()
    .Using<UserContext>()
    .Refine(val => val.Length > 3, "Too short")                    // No context
    .Refine((val, ctx) => val != ctx.BannedEmail, "Email banned"); // With context
```

---

## Async Refinement with Context

Use `RefineAsync` for async validation logic:

```csharp
Z.String()
    .Email()
    .Using<UserContext>()
    .RefineAsync(
        async (email, ctx, ct) => !await ctx.Repo.EmailExistsAsync(email, ct),
        message: "Email already taken",
        code: "email_exists"
    );
```

RefineAsync can access:
- The validated value
- The context data
- The CancellationToken for async operations

**Object Schema Example:**

```csharp
Z.Schema<User>()
    .Property(u => u.Name, Z.String().MinLength(3))
    .Using<UserContext>()
    .Property(u => u.Email,
        Z.String()
            .Email()
            .Using<UserContext>()
            .RefineAsync(async (email, ctx, ct) =>
                !await ctx.Repo.EmailExistsAsync(email, ct),
                "Email already registered"));
```

---

## Context-Aware Conditionals

Use context in `.If()` conditions:

```csharp
Z.Schema<User>()
    .Property(u => u.Password, s => s.MinLength(8))
    .Using<SecurityContext>()
    .If((user, ctx) => ctx.RequireStrongPassword, s => s
        .Property(u => u.Password, p => p.MinLength(12).MaxLength(100)));
```

`.If()` supports both value-only and value+context predicates:

```csharp
// Value-only predicate (no context needed)
.If(user => user.Type == "admin", s => s
    .Property(u => u.Name, n => n.MinLength(5)))

// Value + context predicate
.If((user, ctx) => ctx.IsStrictMode, s => s
    .Property(u => u.Email, e => e.MinLength(10)))
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
        Z.Schema<User>()
            .Using<UserContext>()
            .Property(u => u.Email,
                Z.String()
                    .Email()
                    .Using<UserContext>()
                    .Refine((e, ctx) => !ctx.EmailExists, "Email taken"));

    [HttpPost]
    public async Task<IActionResult> Create(User user, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(
            user,
            UserSchema,
            b => b.WithCancellation(ct));

        return result.ToActionResult(valid => Ok(new { Message = "Created" }));
    }
}
```

You can also keep a context-aware schema in an `ISchema<T>` variable and validate it through `IZetaValidator`; context is still factory-resolved from the schema.

You can also customize `TimeProvider` (or any other builder option):

```csharp
var result = await _validator.ValidateAsync(
    user,
    UserSchema,
    b => b.WithCancellation(ct).WithTimeProvider(fakeTimeProvider));
```

---

## Context Factory Best Practices

### Handle Soft Failures

Factory exceptions propagate as HTTP 500. For expected failures, return a context that causes validation to fail:

```csharp
public async ValueTask<UserContext> BuildContextAsync(User input, IServiceProvider services, CancellationToken ct)
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
public async ValueTask<UserContext> BuildContextAsync(User input, ...)
{
    return new UserContext(
        EmailExists: await _repo.EmailExistsAsync(input.Email, ct)
    );
}

// Avoid - loading unnecessary data
public async ValueTask<UserContext> BuildContextAsync(User input, ...)
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

public async ValueTask<ProductContext> BuildContextAsync(Order input, ...)
{
    var categoryIds = input.Items.Select(i => i.CategoryId).Distinct();
    var productIds = input.Items.Select(i => i.ProductId).Distinct();

    var categories = await _repo.CategoriesExistAsync(categoryIds, ct);
    var prices = await _repo.GetPricesAsync(productIds, ct);

    return new ProductContext(categories, prices);
}
```
