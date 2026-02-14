# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Zeta is a composable, type-safe, async-first validation framework for .NET inspired by Zod. It uses schema-first validation with a Result pattern (no exceptions for validation failures).

## Build and Test Commands

```bash
# Build the entire solution
dotnet build

# Run all tests
dotnet test

# Run a single test by name
dotnet test --filter "FullyQualifiedName~StringSchemaTests.Email_ValidEmail_Succeeds"

# Run tests in a specific project
dotnet test tests/Zeta.Tests

# Run benchmarks
dotnet run --project benchmarks/Zeta.Benchmarks -c Release

# Run the sample API
dotnet run --project samples/Zeta.Sample.Api
```

## Architecture

### Solution Structure
- `src/Zeta/` - Core validation library (no dependencies)
- `src/Zeta.AspNetCore/` - ASP.NET Core integration (Minimal APIs and Controllers)
- `tests/Zeta.Tests/` - Core library tests (xUnit)
- `tests/Zeta.AspNetCore.Tests/` - ASP.NET integration tests
- `samples/Zeta.Sample.Api/` - Example API demonstrating usage
- `benchmarks/Zeta.Benchmarks/` - Performance benchmarks vs FluentValidation/DataAnnotations

### Core Types (src/Zeta/Core/)
- `ISchema<T>` - Contextless validation interface. Returns `Result<T>`.
- `ISchema<T, TContext>` - Context-aware validation interface (separate, no inheritance from `ISchema<T>`). Returns `Result`.
- `Result<T>` - Discriminated result type with `IsSuccess`, `Value`, `Errors`, and monadic operations (`Map`, `Then`, `Match`)
- `ValidationError(Path, Code, Message)` - Error record with dot-notation path (e.g., `user.address.street`)
- `ValidationContext<TData>` - Contains typed context data, path tracking, CancellationToken & TimeProvider
- `ValidationContext` - Contains path tracking, CancellationToken & TimeProvider

### Rule System (src/Zeta/Rules/)
- `IValidationRule<T>` - Context-free sync validation rule using `ValidationExecutionContext`
- `IValidationRule<T, TContext>` - Context-aware sync validation rule using `ValidationContext<TContext>`
- `IAsyncValidationRule<T>` / `IAsyncValidationRule<T, TContext>` - Async variants
- `RuleEngine<T>` - Executes context-free rules for contextless schemas
- `ContextRuleEngine<T, TContext>` - Executes context-aware rules
- `DelegateValidationRule<T>` / `DelegateValidationRule<T, TContext>` - Delegate wrappers for inline rules

### Static Validators (src/Zeta/Validation/)
Shared validation logic used by both contextless and context-aware schemas:
- `StringValidators` - MinLength, MaxLength, Email, Url, Regex, etc.
- `NumericValidators` - Min, Max, Positive, Negative, Precision, etc.
- `CollectionValidators` - MinLength, MaxLength, NotEmpty for arrays/lists

### Schema Types (src/Zeta/Schemas/)
All schemas are created via the static `Z` class entry point as contextless schemas:
- `Z.String()` returns `StringSchema` which implements `ISchema<string>`
- Use `.Using<TContext>()` to promote to context-aware when needed

Schema types: `StringSchema`, `IntSchema`, `DoubleSchema`, `DecimalSchema`, `BoolSchema`, `GuidSchema`, `DateTimeSchema`, `DateOnlySchema`, `TimeOnlySchema`, `ObjectSchema`, `CollectionSchema`

### Key Patterns

**Fluent Method Chaining**: Schemas return `this` from validation methods for chaining:
```csharp
Z.String().MinLength(3).MaxLength(100).Email()
```

**ObjectSchema Field Validation**: Uses expression trees to extract property names, auto-camelCases them for error paths. Use fluent inline builders for most fields:
```csharp
// Default: Fluent inline builders
Z.Object<User>()
    .Field(u => u.Email, s => s.Email().MinLength(5))  // Error path: "email"
    .Field(u => u.Age, s => s.Min(18).Max(100))
    .Field(u => u.Price, s => s.Positive().Precision(2))

// Nullable value types — null skips validation automatically
Z.Object<User>()
    .Field(u => u.OptionalAge, s => s.Min(0).Max(120))     // int? — null OK
    .Field(u => u.OptionalBalance, s => s.Positive())        // decimal? — null OK
    .Field(u => u.Bio, s => s.MaxLength(500).Nullable())     // string? — call .Nullable()

// Supported types: string, int, double, decimal, bool, Guid, DateTime, DateOnly, TimeOnly

// For composability: pre-built schemas when reusing across multiple objects
var addressSchema = Z.Object<Address>()
    .Field(a => a.Street, s => s.MinLength(3))
    .Field(a => a.ZipCode, s => s.Regex(@"^\d{5}$"));

Z.Object<User>()
    .Field(u => u.Email, s => s.Email())
    .Field(u => u.Address, addressSchema)  // Reuse nested schema, path: "address.street"

// Collection fields with .Each() for element validation (RFC 003)
Z.Object<User>()
    .Field(u => u.Roles, roles => roles
        .Each(r => r.MinLength(3).MaxLength(50))  // Validate each string element
        .MinLength(1).MaxLength(10))  // Validate collection size

// Standalone collections
Z.Collection<string>()  // Parameterless, generic type parameter
    .Each(s => s.Email())  // Apply validation to each element
    .MinLength(1)          // Collection-level validation

// For complex nested objects, pass a pre-built schema
var orderItemSchema = Z.Object<OrderItem>()
    .Field(i => i.ProductId, s => s /* Guid validation */)
    .Field(i => i.Quantity, s => s.Min(1));

Z.Collection(orderItemSchema)  // Pass element schema for complex types
    .MinLength(1)
```

**Context Promotion**: Use `.Using<TContext>()` to create a context-aware schema. Rules, fields, and conditionals from the contextless schema are automatically transferred:
```csharp
Z.String()
    .Email()
    .MinLength(5)
    .Using<UserContext>()  // Email and MinLength rules are preserved
    .Refine((email, ctx) => email != ctx.BannedEmail, "Email banned")
    .RefineAsync(async (email, ctx, ct) =>
        !await ctx.Repo.EmailExistsAsync(email, ct), "Email exists");
```

**Context Factory Delegate**: Use `.Using<TContext>(factory)` to embed context creation directly on the schema:
```csharp
Z.Object<CreateOrderRequest>()
    .Using<OrderContext>(async (value, sp, ct) => new OrderContext {
        HasAccess = await sp.GetRequiredService<IPermissionService>().CheckAccessAsync(value.CustomerId, ct)
    })
    .Field(x => x.CustomerId, x => x.NotEmpty())
```

**Async Refinement**: Context-aware schemas support `RefineAsync` for async validation with access to context data and CancellationToken.

**Schema Bridging**: `SchemaAdapter<T, TContext>` wraps contextless `ISchema<T>` for use in context-aware object schemas.

**Conditional Validation**: `.If()` on all schema types enables guard-style conditional validation:
```csharp
// Value schemas — apply rules only when predicate matches
Z.Int()
    .If(v => v >= 18, s => s.Max(65));

// Object schemas — conditional field validation
Z.Object<User>()
    .If(u => u.Type == "admin", s => s
        .Field(u => u.Name, n => n.MinLength(5)));

// Collection schemas — conditional collection rules
Z.Collection<string>()
    .If(items => items.Count > 0, s => s.MinLength(2));

// Multiple guards and nesting
Z.Int()
    .If(v => v >= 0, s => s
        .If(v => v >= 18, inner => inner.Max(100)));

// Context-aware: value-only or value+context predicates
Z.String()
    .Using<StrictContext>()
    .If((v, ctx) => ctx.IsStrict, s => s.MinLength(10));
```

**Type Assertions**: `.As<TDerived>()` on ObjectSchema enables polymorphic type narrowing. Use `.If(predicate, ...)` with `.As<TDerived>()` for type-specific branches:
```csharp
// Explicit: type assertion with .As()
var schema = Z.Object<IAnimal>();
schema.As<Dog>();  // Fails with type_mismatch if not a Dog

// Type-narrowed conditional branches
Z.Object<IAnimal>()
    .If(x => x is Dog, dog => dog.As<Dog>().Field(x => x.WoofVolume, x => x.Min(0).Max(100)))
    .If(x => x is Cat, cat => cat.As<Cat>().Field(x => x.ClawSharpness, x => x.Min(1).Max(10)));
```

### ASP.NET Core Integration (src/Zeta.AspNetCore/)
- `IZetaValidator` - Injectable service for manual validation in controllers
- `ContextlessValidationFilter<T>` - Minimal API endpoint filter for contextless schemas
- `ValidationFilter<T, TContext>` - Minimal API endpoint filter for context-aware schemas
- Context creation is handled by inline factory delegates on schemas via `.Using<TContext>(factory)`

## Design Principles

1. **Async by default** - All validation uses `ValueTask<Result<T>>`, no separate sync paths
2. **No exceptions for control flow** - Validation failures return `Result<T>.Failure()`, never throw
3. **Required by default** - Use `.Nullable()` to allow null values. Nullable value type fields (`int?`, etc.) in object schemas automatically skip validation when null
4. **Path-aware errors** - Errors include full path: `"items[0].name"`, `"address.street"`
5. **Error aggregation** - Collects all errors, no short-circuiting

## Known Behaviors

### Nullable Handling
- `.Nullable()` on any schema makes null a valid value (skips inner validation when null)
- **Nullable value type fields** (`int?`, `double?`, `decimal?`, `bool?`, `Guid?`, `DateTime?`, `DateOnly?`, `TimeOnly?`): Null values automatically skip validation. No `.Nullable()` needed. Non-null values are validated normally.
- **Nullable reference type fields** (`string?`): Null flows through to the schema's `ValidateAsync`. Call `.Nullable()` on the schema if null should pass.
- `ISchema<T>` type parameter is always non-nullable (`ISchema<int>`, never `ISchema<int?>`)
- Field validators bridge the gap for value types via an `isNull` predicate (source-generated)

### Validation Order
ObjectSchema validates in order: Fields → Type Assertions (`.As()`) → Conditionals (`.If()`) → Rules (`.Refine()`)

### Context Factory Failures
Factory exceptions propagate as HTTP 500, not validation errors. Handle soft failures by returning a context that causes validation to fail.

### Required Semantics
- Fields are required (non-null) by default
- Exception: nullable value type fields (`int?`, etc.) — null skips validation automatically
- `.Nullable()` on a schema allows null to pass validation
- `.NotEmpty()` on strings = not whitespace

### Context Promotion
- Schemas are always created contextless via `Z.String()`, `Z.Int()`, etc.
- Contextless `Refine()` uses `Func<T, bool>` (single argument)
- Use `.Using<TContext>()` to create a context-aware schema
- Use `.Using<TContext>(factory)` to also embed a factory delegate for creating context data
- `.Using<TContext>()` is an **instance method** on each schema class (e.g., `StringSchema.Using<TContext>()`)
- **Rules, fields, and conditionals are automatically transferred** when calling `.Using()`
- For ObjectSchema: `.Using<TContext>()` requires only the context type parameter (T is already known)
- After calling `.Using()`, use context-aware `Refine((val, ctx) => ...)` with two arguments
- `.Using()` returns typed schemas: `StringSchema<TContext>`, `IntSchema<TContext>`, `ObjectSchema<T, TContext>`, etc.
- Factory delegate signature: `Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>`

### Interface Separation
- `ISchema<T>` and `ISchema<T, TContext>` are completely separate interfaces (no inheritance relationship)
- Contextless schemas implement `ISchema<T>` only
- Context-aware schemas implement `ISchema<T, TContext>` only
- Use `SchemaAdapter<T, TContext>` to bridge contextless schemas into context-aware contexts

### Adding Validation Methods
When adding a new validation method to a contextless schema type (e.g., `StringSchema.MyMethod()`), you should also add the same method to the corresponding context-aware schema type (e.g., `StringSchema<TContext>.MyMethod()`). This ensures API consistency between contextless and context-aware schemas.

Example: If adding `StringSchema.Foo()`, also add to `StringSchema<TContext>`:
```csharp
public StringSchema<TContext> Foo(...)
{
    Use(new RefinementRule<string, TContext>((val, ctx) => ...));
    return this;
}
```

### After changes
- Then adding schema features all features should support fluent builder for primativ types. And accepts ISchema<T> and ISchema<T, TContext> for object fields. If context aware schema is provided, the full schema must be context aware too.
- Add a section in CHANGELOG.md below "Next release". Versions are managed with tags in git
