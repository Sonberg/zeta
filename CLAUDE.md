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
- `ISchema<T>` / `ISchema<T, TContext>` - Core validation abstraction. Every schema implements this.
- `Result<T>` - Discriminated result type with `IsSuccess`, `Value`, `Errors`, and monadic operations (`Map`, `Then`, `Match`)
- `ValidationError(Path, Code, Message)` - Error record with dot-notation path (e.g., `user.address.street`)
- `ValidationContext<TData>` - Contains typed context data and execution context
- `ValidationExecutionContext` - Path tracking, IServiceProvider access, CancellationToken

### Schema Types (src/Zeta/Schemas/)
All schemas are created via the static `Z` class entry point. Each schema type has two variants:
- `Z.String()` - No context version, implements `ISchema<string>`
- `Z.String<TContext>()` - Context-aware version, implements `ISchema<string, TContext>`

Schema types: `StringSchema`, `IntSchema`, `DoubleSchema`, `DecimalSchema`, `BoolSchema`, `GuidSchema`, `DateTimeSchema`, `DateOnlySchema`, `TimeOnlySchema`, `ObjectSchema`, `ArraySchema`, `ListSchema`, `NullableSchema`

### Key Patterns

**Fluent Method Chaining**: Schemas return `this` from validation methods for chaining:
```csharp
Z.String().MinLength(3).MaxLength(100).Email()
```

**ObjectSchema Field Validation**: Uses expression trees to extract property names, auto-camelCases them for error paths:
```csharp
Z.Object<User>()
    .Field(u => u.Email, Z.String().Email())  // Error path: "email"
    .Field(u => u.Address, addressSchema)      // Nested: "address.street"
```

**Context Adaptation**: `SchemaContextAdapter<T, TContext>` wraps context-free schemas for use in context-aware object schemas.

**Conditional Validation**: `.When()` on ObjectSchema enables dependent field validation via `ConditionalBuilder`.

### ASP.NET Core Integration (src/Zeta.AspNetCore/)
- `IZetaValidator` - Injectable service for manual validation in controllers
- `ValidationFilter` - Minimal API endpoint filter for `.WithValidation(schema)`
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
