# Zeta

[![Build](https://github.com/sonberg/zeta/actions/workflows/ci.yml/badge.svg)](https://github.com/Sonberg/zeta/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

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

## Changelog

See [`CHANGELOG.md`](./CHANGELOG.md).
