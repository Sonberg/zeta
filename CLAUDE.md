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
- `ValidationContext<TData>` - Contains typed context data and execution context
- `ValidationExecutionContext` - Path tracking, IServiceProvider access, CancellationToken

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
- Use `.WithContext<TContext>()` to promote to context-aware when needed

Schema types: `StringSchema`, `IntSchema`, `DoubleSchema`, `DecimalSchema`, `BoolSchema`, `GuidSchema`, `DateTimeSchema`, `DateOnlySchema`, `TimeOnlySchema`, `ObjectSchema`, `ArraySchema`, `ListSchema`, `NullableSchema`

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

// Supported types: string, int, double, decimal, bool, Guid, DateTime, DateOnly, TimeOnly

// For composability: pre-built schemas when reusing across multiple objects
var addressSchema = Z.Object<Address>()
    .Field(a => a.Street, s => s.MinLength(3))
    .Field(a => a.ZipCode, s => s.Regex(@"^\d{5}$"));

Z.Object<User>()
    .Field(u => u.Email, s => s.Email())
    .Field(u => u.Address, addressSchema)  // Reuse nested schema, path: "address.street"
```

**Context Promotion**: Use `.WithContext<TContext>()` to create a context-aware schema. Rules, fields, and conditionals from the contextless schema are automatically transferred:
```csharp
Z.String()
    .Email()
    .MinLength(5)
    .WithContext<UserContext>()  // Email and MinLength rules are preserved
    .Refine((email, ctx) => email != ctx.BannedEmail, "Email banned")
    .RefineAsync(async (email, ctx, ct) =>
        !await ctx.Repo.EmailExistsAsync(email, ct), "Email exists");
```

**Async Refinement**: Context-aware schemas support `RefineAsync` for async validation with access to context data and CancellationToken.

**Schema Bridging**: `SchemaAdapter<T, TContext>` wraps contextless `ISchema<T>` for use in context-aware object schemas.

**Conditional Validation**: `.When()` on ObjectSchema enables dependent field validation via `ConditionalBuilder`:
- `.Require(x => x.Field)` - Ensures field is not null
- `.Field(x => x.Field, schema)` - Validates field with a schema
- `.Select(x => x.Field, s => s.MinLength(3))` - Inline schema builder for string, int, double, decimal types

### ASP.NET Core Integration (src/Zeta.AspNetCore/)
- `IZetaValidator` - Injectable service for manual validation in controllers
- `ContextlessValidationFilter<T>` - Minimal API endpoint filter for contextless schemas
- `ValidationFilter<T, TContext>` - Minimal API endpoint filter for context-aware schemas
- `IValidationContextFactory<TInput, TContext>` - Async context loading before validation (scanned from assemblies via `AddZeta(assembly)`)

## Design Principles

1. **Async by default** - All validation uses `ValueTask<Result<T>>`, no separate sync paths
2. **No exceptions for control flow** - Validation failures return `Result<T>.Failure()`, never throw
3. **Required by default** - Use `.Nullable()` extension to make optional
4. **Path-aware errors** - Errors include full path: `"items[0].name"`, `"address.street"`
5. **Error aggregation** - Collects all errors, no short-circuiting

## Known Behaviors

### Nullable vs Optional
- `.Nullable()` and `.Optional()` are functionally equivalent
- `.Nullable()` = "null is a valid value"
- `.Optional()` = "field may be omitted" (clearer intent for PATCH)
- Both skip inner validation when value is null

### Validation Order
ObjectSchema validates in order: Fields → Conditionals (`.When()`) → Rules (`.Refine()`)

### Context Factory Failures
Factory exceptions propagate as HTTP 500, not validation errors. Handle soft failures by returning a context that causes validation to fail.

### Required Semantics
- Fields are required (non-null) by default
- `.Require()` in conditionals = not null check
- `.NotEmpty()` on strings = not whitespace

### Context Promotion
- Schemas are always created contextless via `Z.String()`, `Z.Int()`, etc.
- Contextless `Refine()` uses `Func<T, bool>` (single argument)
- Use `.WithContext<TContext>()` to create a context-aware schema
- `.WithContext<TContext>()` is an **instance method** on each schema class (e.g., `StringSchema.WithContext<TContext>()`)
- **Rules, fields, and conditionals are automatically transferred** when calling `.WithContext()`
- For ObjectSchema: `.WithContext<TContext>()` requires only the context type parameter (T is already known)
- After calling `.WithContext()`, use context-aware `Refine((val, ctx) => ...)` with two arguments
- `.WithContext()` returns typed schemas: `StringSchema<TContext>`, `IntSchema<TContext>`, `ObjectSchema<T, TContext>`, etc.

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
