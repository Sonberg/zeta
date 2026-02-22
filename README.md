# Zeta

[![Build](https://github.com/sonberg/zeta/actions/workflows/ci.yml/badge.svg)](https://github.com/Sonberg/zeta/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![GitHub stars](https://img.shields.io/github/stars/Sonberg/zeta.svg?style=social)](https://github.com/Sonberg/zeta/stargazers)
[![codecov](https://codecov.io/gh/Sonberg/zeta/branch/main/graph/badge.svg)](https://codecov.io/gh/Sonberg/zeta) [![NuGet](https://img.shields.io/nuget/v/Zeta.AspNetCore.svg)](https://www.nuget.org/packages/Zeta.AspNetCore)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Zeta.AspNetCore.svg)](https://www.nuget.org/packages/Zeta.AspNetCore)

Zeta is a schema-first validation framework for .NET with a fluent, async-first API.

```csharp
using Zeta;

var schema = Z.Schema<User>()
    .Property(x => x.Email, s => s.Email())
    .Property(x => x.Age, s => s.Min(18));

var result = await schema.ValidateAsync(new User("alice@example.com", 21));

if (!result.IsSuccess)
{
    foreach (var error in result.Errors)
        Console.WriteLine($"{error.PathString}: {error.Message}");
}

public sealed record User(string Email, int Age);
```

## Quick Start

```bash
dotnet add package Zeta
```

```bash
# Optional: ASP.NET Core integration (Minimal APIs / Controllers)
dotnet add package Zeta.AspNetCore
```

## Core Examples

### 1. Minimal API Validation

```csharp
using Zeta;
using Zeta.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddZeta();
var app = builder.Build();

var createUserSchema = Z.Schema<CreateUserRequest>()
    .Property(x => x.Email, s => s.Email())
    .Property(x => x.Name, s => s.MinLength(2));

app.MapPost("/users", (CreateUserRequest request) => Results.Ok(request))
    .WithValidation(createUserSchema);

app.Run();

public sealed record CreateUserRequest(string Email, string Name);
```

### 2. Context-Aware Rules with `.Using(...)`

```csharp
var registerSchema = Z.Schema<RegisterRequest>()
    .Using<RegisterContext>(async (input, sp, ct) =>
    {
        var repo = sp.GetRequiredService<IUserRepository>();
        var exists = await repo.EmailExistsAsync(input.Email, ct);
        return new RegisterContext(exists);
    })
    .Property(x => x.Email, s => s.Email())
    .Refine((x, ctx) => !ctx.EmailExists, "Email already exists", "email_exists");

public sealed record RegisterRequest(string Email);
public sealed record RegisterContext(bool EmailExists);
```

### 3. Collection Validation with `.Each(...)`

```csharp
var orderSchema = Z.Schema<CreateOrderRequest>()
    .Property(x => x.Items, items => items
        .Each(i => i
            .Property(v => v.ProductId, s => s.NotEmpty())
            .Property(v => v.Quantity, s => s.Min(1)))
        .MinLength(1));

public sealed record CreateOrderRequest(List<OrderItem> Items);
public sealed record OrderItem(string ProductId, int Quantity);
```

This repository contains:
- Core package source: [`src/Zeta`](./src/Zeta)
- ASP.NET Core integration package source: [`src/Zeta.AspNetCore`](./src/Zeta.AspNetCore)
- Source generators: [`src/Zeta.SourceGenerators`](./src/Zeta.SourceGenerators)
- Tests: [`tests`](./tests)
- Samples: [`samples`](./samples)
- Benchmarks: [`benchmarks`](./benchmarks)

## Package Documentation

- Core validation package (`Zeta`): [`src/Zeta/README.md`](./src/Zeta/README.md)
- ASP.NET Core integration package (`Zeta.AspNetCore`): [`src/Zeta.AspNetCore/README.md`](./src/Zeta.AspNetCore/README.md)
- Guides: [`docs`](./docs)

## Which package should I use?

- `Zeta`: Use in any .NET app (Console, Worker, Blazor, MAUI, libraries).
- `Zeta.AspNetCore`: Add only when you need ASP.NET Core integration (Minimal APIs, Controllers, validation filters).

## Build and Test

```bash
dotnet build
dotnet test
```

## Samples

```bash
# ASP.NET Core sample
dotnet run --project samples/Zeta.Sample.Api

# Blazor sample
dotnet run --project samples/Zeta.Sample.Blazor
```

## Benchmarks

Comparing Zeta against FluentValidation and DataAnnotations on .NET 10 (Apple M2 Pro).

| Method | Mean | Allocated |
|--------|-----:|----------:|
| FluentValidation | 131.2 ns | 600 B |
| FluentValidation (Async) | 230.1 ns | 672 B |
| **Zeta** | **353.2 ns** | **216 B** |
| Zeta (Invalid) | 442.2 ns | 1,048 B |
| DataAnnotations | 627.9 ns | 1,848 B |
| DataAnnotations (Invalid) | 990.5 ns | 2,672 B |
| FluentValidation (Invalid) | 1,923.9 ns | 7,728 B |
| FluentValidation (Invalid Async) | 2,095.5 ns | 7,800 B |

**Key findings:**
- Allocates **64% less memory** than FluentValidation on valid input (216 B vs 600 B)
- Allocates **7.4x less memory** than FluentValidation on invalid input (1,048 B vs 7,728 B)
- **4.4x faster** than FluentValidation when validation fails (442 ns vs 1,924 ns)
- **2.2x faster** than DataAnnotations when validation fails (442 ns vs 991 ns)

Run benchmarks:

```bash
dotnet run --project benchmarks/Zeta.Benchmarks -c Release
```

## Changelog

See [`CHANGELOG.md`](./CHANGELOG.md).
