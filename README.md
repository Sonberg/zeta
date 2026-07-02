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

# Optional: FastEndpoints integration
dotnet add package Zeta.FastEndpoints
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
- FastEndpoints integration package source: [`src/Zeta.FastEndpoints`](./src/Zeta.FastEndpoints)
- Source generators: [`src/Zeta.SourceGenerators`](./src/Zeta.SourceGenerators)
- Tests: [`tests`](./tests)
- Samples: [`samples`](./samples)
- Benchmarks: [`benchmarks`](./benchmarks)

## Package Documentation

- Core validation package (`Zeta`): [`src/Zeta/README.md`](./src/Zeta/README.md)
- ASP.NET Core integration package (`Zeta.AspNetCore`): [`src/Zeta.AspNetCore/README.md`](./src/Zeta.AspNetCore/README.md)
- FastEndpoints integration package (`Zeta.FastEndpoints`): [`src/Zeta.FastEndpoints/README.md`](./src/Zeta.FastEndpoints/README.md)
- Guides: [`docs`](./docs)

## Which package should I use?

- `Zeta`: Use in any .NET app (Console, Worker, Blazor, MAUI, libraries).
- `Zeta.AspNetCore`: Add only when you need ASP.NET Core integration (Minimal APIs, Controllers, validation filters).
- `Zeta.FastEndpoints`: Add when you use [FastEndpoints](https://fast-endpoints.com) as your web framework.

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

# FastEndpoints sample
dotnet run --project samples/Zeta.Sample.FastEndpoints.Api
```

## Benchmarks

Comparing Zeta against FluentValidation and DataAnnotations on .NET 10 (Apple M2 Pro).

### Simple object (3 flat properties)

| Method | Mean | Allocated |
|--------|-----:|----------:|
| FluentValidation | 137.6 ns | 600 B |
| FluentValidation (Async) | 236.3 ns | 672 B |
| **Zeta** | **287.6 ns** | **72 B** |
| Zeta (Invalid) | 515.9 ns | 1,424 B |
| DataAnnotations | 605.3 ns | 1,848 B |
| DataAnnotations (Invalid) | 1,015.9 ns | 2,672 B |
| FluentValidation (Invalid) | 1,913.6 ns | 7,312 B |
| FluentValidation (Invalid Async) | 2,068.4 ns | 7,384 B |

- Allocates **88% less memory** than FluentValidation on valid input (72 B vs 600 B)
- **3.7x faster** than FluentValidation when validation fails (516 ns vs 1,914 ns), with **5.1x less** memory (1,424 B vs 7,312 B)

### Complex object graph (nested object + collection of objects + conditional + cross-field rule)

A realistic e-commerce order: email, nested shipping address, a list of line items, a nullable discount code, and a cross-field total rule. DataAnnotations is excluded — `TryValidateObject` does not recurse into nested objects or collection elements.

| Method | Mean | Allocated |
|--------|-----:|----------:|
| **Zeta** | **1.606 µs** | **952 B** |
| FluentValidation | 1.778 µs | 4,904 B |
| Zeta (Invalid) | 2.280 µs | 6,856 B |
| FluentValidation (Invalid) | 5.806 µs | 18,136 B |

- **5.1x less** memory than FluentValidation on valid input (952 B vs 4,904 B)
- **2.5x faster** than FluentValidation when validation fails (2.28 µs vs 5.81 µs), with **2.6x less** memory (6,856 B vs 18,136 B)

Run benchmarks:

```bash
dotnet run --project benchmarks/Zeta.Benchmarks -c Release              # all
dotnet run --project benchmarks/Zeta.Benchmarks -c Release -- --filter '*Complex*'
```

## Changelog

See [`CHANGELOG.md`](./CHANGELOG.md).
