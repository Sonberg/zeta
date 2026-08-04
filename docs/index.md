---
layout: home

hero:
  name: Zeta
  text: Validation for .NET, defined as schemas
  tagline: Composable, type-safe and async-first. Schemas are values you can reuse and combine — validation failures come back as results, not exceptions.

features:
  - title: Schema-first
    details: Define validation once as a reusable schema object instead of scattering attributes across your DTOs. Schemas compose, nest, and branch.
  - title: Async by default
    details: Every rule can be async. There is no separate sync path to keep in sync — hit a database or an HTTP API from a rule and it just works.
  - title: Results, not exceptions
    details: Validation returns Result&lt;T&gt;. Invalid input is an ordinary value you pattern-match on, not a thrown exception you have to catch.
  - title: Path-aware errors
    details: Every error carries a JSONPath location like $.items[0].quantity, so client-side field mapping is exact even for deeply nested graphs.
  - title: Context-aware rules
    details: Promote a schema with .Using&lt;TContext&gt;() to load data once per validation run through DI, then read it from any rule in the tree.
  - title: Allocation-light
    details: 72 B allocated on valid input versus 600 B for FluentValidation, and 3.7x faster when validation fails. Benchmarks are in the repo.
---

## What is Zeta?

Zeta is a validation framework for .NET inspired by [Zod](https://zod.dev/). You describe the shape
and constraints of your data as a **schema**, then validate values against it:

```csharp
using Zeta;

var schema = Z.Schema<User>()
    .Property(u => u.Email, s => s.Email())
    .Property(u => u.Age, s => s.Min(18));

var result = await schema.ValidateAsync(new User("alice@example.com", 21));

if (!result.IsSuccess)
{
    foreach (var error in result.Errors)
        Console.WriteLine($"{error.PathString}: {error.Message}");
}

public sealed record User(string Email, int Age);
```

The schema is an ordinary value. Store it in a static field, pass it around, nest it inside another
schema, or promote it to a context-aware variant — it stays immutable, so reuse is always safe.

## Why not attributes?

DataAnnotations and similar attribute-based approaches tie validation to the type declaration. That
works until you need the same DTO validated differently in two places, or a rule that depends on a
database lookup, or an error path that matches your JSON payload. Zeta separates the *rules* from
the *type*, which makes all three straightforward.

## Install

```bash
dotnet add package Zeta
```

Requires **.NET 6 or later**. The [ASP.NET Core](/aspnetcore) and [FastEndpoints](/fastendpoints)
integration packages require .NET 8+.

Head to [Getting started](/getting-started) for a walkthrough, or jump to the
[validator reference](/validators) if you just want the list of built-in rules.
