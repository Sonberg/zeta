# Collections Guide

This guide covers validation of arrays, lists, and other collection types in Zeta.

## Table of Contents

- [Basic Collection Validation](#basic-collection-validation)
- [Element Validation with `.Each()`](#element-validation-with-each)
- [Collection-Level Validation](#collection-level-validation)
- [Combining Element and Collection Rules](#combining-element-and-collection-rules)
- [Collections in Object Schemas](#collections-in-object-schemas)
- [Nested Object Collections](#nested-object-collections)
- [Multiple `.Each()` Calls](#multiple-each-calls)
- [Context-Aware Collections](#context-aware-collections)
- [Error Paths](#error-paths)
- [Best Practices](#best-practices)

## Basic Collection Validation

Create a collection schema using `Z.Collection<T>()` with the element type:

```csharp
// Collection of strings
var schema = Z.Collection<string>();
var result = await schema.ValidateAsync(["a", "b", "c"]);
// ✓ Success

// Collection of integers
var schema = Z.Collection<int>();
var result = await schema.ValidateAsync([1, 2, 3]);
// ✓ Success
```

**Supported Element Types:**
- Primitives: `string`, `int`, `double`, `decimal`, `bool`, `Guid`
- Date/Time: `DateTime`, `DateOnly`, `TimeOnly`
- Complex objects: Any class type

## Element Validation with `.Each()`

Use `.Each()` to apply validation rules to every element in the collection:

```csharp
// Validate each string element
var schema = Z.Collection<string>()
    .Each(s => s.Email());

await schema.ValidateAsync(["user@example.com", "admin@example.com"]);
// ✓ Success

await schema.ValidateAsync(["user@example.com", "invalid"]);
// ✗ Failure: email validation failed at [1]
```

### Primitive Type Validation

```csharp
// Strings
Z.Collection<string>()
    .Each(s => s.MinLength(3).MaxLength(50).NotEmpty())

// Integers
Z.Collection<int>()
    .Each(n => n.Min(0).Max(100))

// Decimals with precision
Z.Collection<decimal>()
    .Each(d => d.Positive().Precision(2))

// Dates
Z.Collection<DateTime>()
    .Each(dt => dt.Past().MinAge(18))

// GUIDs
Z.Collection<Guid>()
    .Each(g => g.NotEmpty())
```

### Custom Refinements

Add custom validation logic with `.Refine()`:

```csharp
Z.Collection<string>()
    .Each(s => s
        .MinLength(3)
        .Refine(v => v.StartsWith("A"), "Must start with A"))

Z.Collection<int>()
    .Each(n => n
        .Min(0)
        .Refine(v => v % 2 == 0, "Must be even"))
```

## Collection-Level Validation

Apply constraints to the collection itself (size, emptiness):

```csharp
// Minimum length
Z.Collection<string>()
    .MinLength(1)  // At least 1 element

// Maximum length
Z.Collection<string>()
    .MaxLength(10)  // At most 10 elements

// Exact length
Z.Collection<int>()
    .Length(5)  // Exactly 5 elements

// Not empty (equivalent to MinLength(1))
Z.Collection<string>()
    .NotEmpty()

// Custom collection-level rules
Z.Collection<int>()
    .Refine(
        collection => collection.Sum() <= 100,
        "Total must not exceed 100"
    )
```

## Combining Element and Collection Rules

Chain `.Each()` with collection-level validations:

```csharp
Z.Collection<string>()
    .Each(s => s.Email().MinLength(5))  // Element rules
    .MinLength(1)                       // Collection rules
    .MaxLength(10)

Z.Collection<int>()
    .Each(n => n.Min(0).Max(100))
    .NotEmpty()
    .MaxLength(50)
```

**Validation Order:**
1. Collection-level rules execute first
2. Element validation runs if collection rules pass
3. All errors are collected (no short-circuiting)

## Collections in Object Schemas

Use fluent builders for collection fields in object schemas:

```csharp
public record User(
    string Name,
    List<string> Tags,
    List<string> Emails
);

var schema = Z.Object<User>()
    .Field(u => u.Name, s => s.MinLength(3))
    .Field(u => u.Tags, tags => tags
        .Each(t => t.MinLength(3).MaxLength(50))
        .MinLength(1)
        .MaxLength(10))
    .Field(u => u.Emails, emails => emails
        .Each(e => e.Email())
        .MinLength(1)
        .MaxLength(5));
```

**Important:** The fluent builder syntax only works with `List<T>` properties. For array properties, either:
1. Change the property to `List<T>`
2. Use a pre-built schema (see [Nested Object Collections](#nested-object-collections))

## Nested Object Collections

For collections of complex objects, create a pre-built schema:

```csharp
public record OrderItem(Guid ProductId, int Quantity, string? Notes);
public record Order(Guid CustomerId, List<OrderItem> Items);

// Create element schema
var orderItemSchema = Z.Object<OrderItem>()
    .Field(i => i.ProductId, s => s)
    .Field(i => i.Quantity, s => s.Min(1).Max(100))
    .Field(i => i.Notes, s => s.MaxLength(500).Nullable());

// Use in collection
var orderSchema = Z.Object<Order>()
    .Field(o => o.CustomerId, s => s)
    .Field(o => o.Items, Z.Collection(orderItemSchema)  // Pass pre-built schema
        .MinLength(1)
        .MaxLength(20));

// Validation
var order = new Order(
    CustomerId: Guid.NewGuid(),
    Items: new List<OrderItem>
    {
        new(Guid.NewGuid(), 2, "Gift wrap"),
        new(Guid.NewGuid(), -1, null)  // ✗ Invalid quantity
    }
);

var result = await orderSchema.ValidateAsync(order);
// Error at path: "items[1].quantity" with message "Must be at least 1"
```

### Reusable Nested Schemas

Extract complex schemas for reuse:

```csharp
// Shared address schema
var addressSchema = Z.Object<Address>()
    .Field(a => a.Street, s => s.MinLength(5).MaxLength(200))
    .Field(a => a.City, s => s.MinLength(2).MaxLength(100))
    .Field(a => a.ZipCode, s => s.Regex(@"^\d{5}(-\d{4})?$"));

// Use in multiple places
var customerSchema = Z.Object<Customer>()
    .Field(c => c.Name, s => s.MinLength(2))
    .Field(c => c.Addresses, Z.Collection(addressSchema)
        .MinLength(1)
        .MaxLength(5));

var warehouseSchema = Z.Object<Warehouse>()
    .Field(w => w.Name, s => s.MinLength(3))
    .Field(w => w.Locations, Z.Collection(addressSchema)
        .MinLength(1));
```

## Multiple `.Each()` Calls

Chain multiple `.Each()` calls to compose validations:

```csharp
var schema = Z.Collection<string>()
    .Each(s => s.MinLength(3))      // First transformation
    .Each(s => s.MaxLength(50))     // Second transformation
    .Each(s => s.Regex(@"^[A-Z]")); // Third transformation

// Equivalent to:
Z.Collection<string>()
    .Each(s => s.MinLength(3).MaxLength(50).Regex(@"^[A-Z]"));
```

Each `.Each()` call creates a new schema with the transformed element validation. This enables:
- **Gradual refinement** - Build up validation rules step by step
- **Composition** - Combine validation logic from different sources
- **Conditional application** - Apply certain rules conditionally

```csharp
var schema = Z.Collection<string>()
    .Each(s => s.MinLength(3));

// Conditionally add more validation
if (requireEmail)
{
    schema = schema.Each(s => s.Email());
}
```

## Context-Aware Collections

Use `.WithContext<TContext>()` for async validation with external data:

```csharp
public record ProductContext(IProductRepository Repository);

var schema = Z.Collection<string>()
    .Each(s => s.MinLength(3))                    // Contextless validation
    .WithContext<ProductContext>()                 // Promote to context-aware
    .Each(s => s.RefineAsync(async (sku, ctx, ct) =>
        await ctx.Repository.SkuExistsAsync(sku, ct),
        "SKU does not exist"));

// Validate with context
var context = new ProductContext(repository);
var result = await schema.ValidateAsync(
    ["SKU-001", "SKU-002"],
    new ValidationContext<ProductContext>(context)
);
```

### Context-Aware in Object Schemas

```csharp
var schema = Z.Object<Product>()
    .Field(p => p.Name, s => s.MinLength(3))
    .WithContext<ProductContext>()
    .Field(p => p.RelatedSkus, skus => skus
        .Each(s => s.MinLength(3))
        .WithContext<ProductContext>()              // Collection also needs context
        .Each(s => s.RefineAsync(async (sku, ctx, ct) =>
            await ctx.Repository.SkuExistsAsync(sku, ct),
            "SKU does not exist")));
```

## Error Paths

Collection validation errors include the element index in the path:

```csharp
var schema = Z.Collection<string>()
    .Each(s => s.Email());

var result = await schema.ValidateAsync([
    "valid@example.com",
    "invalid",
    "also@example.com",
    "bad-email"
]);

// Errors:
// - Path: "[1]", Code: "email", Message: "Invalid email format"
// - Path: "[3]", Code: "email", Message: "Invalid email format"
```

### Nested Collection Errors

Paths include full navigation to the error:

```csharp
public record User(string Name, List<Address> Addresses);
public record Address(string Street, string City, string ZipCode);

var schema = Z.Object<User>()
    .Field(u => u.Name, s => s.MinLength(2))
    .Field(u => u.Addresses, Z.Collection(
        Z.Object<Address>()
            .Field(a => a.Street, s => s.MinLength(5))
            .Field(a => a.ZipCode, s => s.Regex(@"^\d{5}$"))
    ).MinLength(1));

var user = new User(
    Name: "John",
    Addresses: new List<Address>
    {
        new("123 Main St", "Boston", "02101"),
        new("Apt", "NYC", "invalid")  // ✗ Street too short, ZipCode invalid
    }
);

var result = await schema.ValidateAsync(user);
// Errors:
// - Path: "addresses[1].street", Code: "min_length"
// - Path: "addresses[1].zipCode", Code: "regex"
```

## Best Practices

### 1. Use Fluent Builders for Simple Cases

```csharp
// ✓ Good - Simple, readable
Z.Object<User>()
    .Field(u => u.Tags, tags => tags
        .Each(t => t.MinLength(3))
        .MaxLength(10))
```

### 2. Extract Complex Element Schemas

```csharp
// ✓ Good - Reusable, testable
var addressSchema = Z.Object<Address>()
    .Field(a => a.Street, s => s.MinLength(5))
    .Field(a => a.ZipCode, s => s.Regex(@"^\d{5}$"));

Z.Object<User>()
    .Field(u => u.Addresses, Z.Collection(addressSchema).MinLength(1))

// ✗ Avoid - Nested and hard to test
Z.Object<User>()
    .Field(u => u.Addresses, Z.Collection(
        Z.Object<Address>()
            .Field(a => a.Street, s => s.MinLength(5))
            .Field(a => a.ZipCode, s => s.Regex(@"^\d{5}$"))
    ).MinLength(1))
```

### 3. Apply Collection Rules First, Then Element Rules

```csharp
// ✓ Good - Fails fast on empty collection
Z.Collection<string>()
    .NotEmpty()                    // Check size first
    .Each(s => s.Email())          // Then validate elements

// ✗ Avoid - Validates elements even if empty
Z.Collection<string>()
    .Each(s => s.Email())
    .NotEmpty()
```

### 4. Use `List<T>` for Fluent Builders

```csharp
// ✓ Good - Works with fluent builder
public record User(string Name, List<string> Tags);

Z.Object<User>()
    .Field(u => u.Tags, tags => tags.Each(t => t.MinLength(3)))

// ✗ Avoid - Arrays don't work with fluent builders
public record User(string Name, string[] Tags);

Z.Object<User>()
    .Field(u => u.Tags, tags => tags.Each(t => t.MinLength(3)))  // Won't compile
```

### 5. Keep Element Validation Simple

```csharp
// ✓ Good - Clear, focused validation
Z.Collection<string>()
    .Each(s => s.Email().MinLength(5).MaxLength(100))

// ✗ Avoid - Too complex, extract to separate schema
Z.Collection<string>()
    .Each(s => s
        .MinLength(5)
        .MaxLength(100)
        .Regex(@"...")
        .Refine(...)
        .Refine(...))
```

### 6. Use Context-Aware Validation Sparingly

```csharp
// ✓ Good - Only when async validation is truly needed
Z.Collection<string>()
    .Each(s => s.MinLength(3))                      // Simple validation
    .WithContext<Context>()
    .Each(s => s.RefineAsync(CheckDatabaseAsync))   // Only for async check

// ✗ Avoid - Unnecessary context promotion
Z.Collection<string>()
    .WithContext<Context>()
    .Each(s => s.MinLength(3))  // No context needed
```

## Common Patterns

### Email List Validation

```csharp
var emailListSchema = Z.Collection<string>()
    .Each(s => s.Email().MinLength(5))
    .MinLength(1)
    .MaxLength(5);
```

### Tag System

```csharp
var tagSchema = Z.Collection<string>()
    .Each(s => s
        .MinLength(2)
        .MaxLength(30)
        .Alphanumeric())
    .MinLength(1)
    .MaxLength(10);
```

### Shopping Cart Items

```csharp
var itemSchema = Z.Object<CartItem>()
    .Field(i => i.ProductId, s => s.NotEmpty())
    .Field(i => i.Quantity, s => s.Min(1).Max(100))
    .Field(i => i.Price, s => s.Positive().Precision(2));

var cartSchema = Z.Collection(itemSchema)
    .MinLength(1)
    .MaxLength(50)
    .Refine(
        items => items.Sum(i => i.Price * i.Quantity) <= 10000m,
        "Cart total must not exceed $10,000"
    );
```

### Role-Based Access

```csharp
var allowedRoles = new[] { "Admin", "Editor", "Viewer" };

var roleSchema = Z.Collection<string>()
    .Each(r => r.Refine(
        role => allowedRoles.Contains(role),
        "Invalid role"))
    .MinLength(1)
    .Refine(
        roles => roles.Distinct().Count() == roles.Count,
        "Roles must be unique"
    );
```

## See Also

- [Fluent Field Builders](FluentFieldBuilders.md) - Using collections in object schemas
- [Validation Context](ValidationContext.md) - Async validation with context
- [Custom Rules](CustomRules.md) - Creating reusable validation rules
- [RFC 003](../rfc/003-rfc-collection-schema.md) - Technical design of `.Each()` method
