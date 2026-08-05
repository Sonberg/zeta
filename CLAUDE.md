# CLAUDE.md

Guidance for Claude Code when working in this repository.

## Project Overview

Zeta is a composable, type-safe, async-first validation framework for .NET inspired by Zod. Schema-first validation with a Result pattern — no exceptions for validation failures.

## Build and Test

```bash
dotnet build
dotnet test
dotnet test --filter "FullyQualifiedName~StringSchemaTests.Email_ValidEmail_Succeeds"  # single test
dotnet run --project benchmarks/Zeta.Benchmarks -c Release                             # benchmarks
dotnet run --project samples/Zeta.Sample.Api                                           # sample API
```

## Solution Structure
- `src/Zeta/` — core library (no dependencies)
- `src/Zeta.AspNetCore/`, `src/Zeta.FastEndpoints/` — web integrations
- `tests/`, `samples/`, `benchmarks/`

## Core Concepts

- `ISchema<T>` (contextless, returns `Result<T>`) and `ISchema<T, TContext>` (context-aware, returns `Result`) are **completely separate** interfaces — no inheritance. Use `SchemaAdapter<T, TContext>` to bridge.
- `Result<T>` — discriminated type with `IsSuccess`, `Value`, `Errors`, and `Map`/`Then`/`Match`.
- `ValidationError(Path, Code, Message)` — JSONPath-aware errors (`$.items[0].name`).
- Schemas are created contextless via the static `Z` entry point (`Z.String()`, `Z.Schema<T>()`, `Z.Collection<T>()`, …). Call `.Using<TContext>()` to promote to context-aware — rules, properties, and conditionals transfer automatically.

## Key Patterns

**Object schemas** — `.Property(...)` uses expression trees to extract names, auto-camelCased for error paths:
```csharp
Z.Schema<User>()
    .Property(u => u.Email, s => s.Email().MinLength(5))     // "$.email"
    .Property(u => u.Age, s => s.Min(18).Max(100))
    .Property(u => u.OptionalAge, s => s.Min(0).Max(120).Nullable())  // int? — call .Nullable() to allow null
    .Property(u => u.Bio, s => s.MaxLength(500).Nullable())  // string? — call .Nullable() to allow null
    .Property(u => u.Address, addressSchema)                 // reuse a pre-built nested schema
```

**Collections / dictionaries** — `.Each()` (and `.EachKey()`/`.EachValue()`) for elements, plus collection-level rules:
```csharp
Z.Collection<string>().Each(s => s.Email()).MinLength(1)
```

**Context promotion & factory**:
```csharp
Z.String().Email().Using<UserContext>()
    .Refine((email, ctx) => email != ctx.BannedEmail, "Email banned")
    .RefineAsync(async (email, ctx, ct) => !await ctx.Repo.EmailExistsAsync(email, ct), "Email exists");

Z.Schema<CreateOrderRequest>()
    .Using<OrderContext>(async (value, sp, ct) => new OrderContext { /* resolve services */ })
    .Property(x => x.CustomerId, x => x.NotEmpty());
```
Factory signature: `Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>`. Contextless `Refine` takes `Func<T, bool>`; context-aware takes `(val, ctx)`.

**Conditionals** — `.If(predicate, schema)` on every schema type (value/object/collection), nestable; context-aware variants accept value-only or `(v, ctx)` predicates.

**Type assertions** — `.As<TDerived>()` on object schemas; prefer branch schemas: `Z.Schema<IAnimal>().If(x => x is Dog, dogSchema)`.

## Design Principles
1. Async by default — all validation is `ValueTask<Result<T>>`, no sync paths.
2. No exceptions for control flow — return `Result<T>.Failure()`.
3. Required by default — `.Nullable()` allows null. This applies uniformly to value-type and reference-type properties alike; without it, null fails with `null_value`.
4. Path-aware errors and full aggregation (no short-circuiting).

## Known Behaviors

- **Nullable**: `.Nullable()` makes null valid on any schema. This is required uniformly — value-type properties (`int?`, `Guid?`, …) do **not** skip validation automatically; without `.Nullable()`, null fails with `null_value` just like reference types (`string?`). `ISchema<T>` is always non-nullable (`ISchema<int>`, never `ISchema<int?>`).
- **Validation order** (object schemas): Properties → Type Assertions (`.As()`) → Conditionals (`.If()`) → Rules (`.Refine()`).
- **Context factory failures** propagate as HTTP 500, not validation errors — return a context that fails validation for soft failures.
- **`.NotEmpty()` on strings** = not whitespace.

## Adding Validation Methods

Value-schema validators (string, int, double, decimal, bool, Guid, enum, DateTime, DateOnly, TimeOnly) are **extension methods** shared by both variants — add to the matching `*SchemaExtensions` class in `src/Zeta/Schemas/` (namespace `Zeta`). The extension hangs off `IValueSchema<T, TSelf>` (in `src/Zeta/Core/`); its `AppendRule(IValidationRule<T>)` is inherited from the base classes. A single **contextless** rule struct (or inline `RefinementRule<T>`) backs both flavours — the context-aware base wraps it in `ContextlessRuleAdapter<T, TContext>`. Do not add `XRule<TContext>` variants.

```csharp
public static TSelf Foo<TSelf>(this IValueSchema<string, TSelf> schema, string? message = null)
    where TSelf : IValueSchema<string, TSelf>
    => schema.AppendRule(new FooRule(message));
```

`Object`/`Collection`/`Dictionary` schemas are **not** `IValueSchema` — their fluent methods stay as instance/generated members (`Property`/`Each` overloads are source-generated).

## After Changes
- New schema features must support fluent builders for primitive types and accept `ISchema<T>` / `ISchema<T, TContext>` for object properties. A context-aware property makes the whole schema context-aware.
- Add a note under "Next release" in CHANGELOG.md. Versions are managed with git tags.
