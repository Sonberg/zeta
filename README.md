# Zeta

[![Build](https://github.com/sonberg/zeta/actions/workflows/ci.yml/badge.svg)](https://github.com/Sonberg/zeta/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![GitHub stars](https://img.shields.io/github/stars/Sonberg/zeta.svg?style=social)](https://github.com/Sonberg/zeta/stargazers)
[![codecov](https://codecov.io/gh/Sonberg/zeta/branch/main/graph/badge.svg)](https://codecov.io/gh/Sonberg/zeta) [![NuGet](https://img.shields.io/nuget/v/Zeta.AspNetCore.svg)](https://www.nuget.org/packages/Zeta.AspNetCore)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Zeta.AspNetCore.svg)](https://www.nuget.org/packages/Zeta.AspNetCore)

Zeta is a schema-first validation framework for .NET with a fluent API.

```csharp
using Zeta;

var userSchema = Z.Schema<User>()
    .Property(x => x.Email, s => s.Email())
    .Property(x => x.Age, s => s.Min(18));

var result = await userSchema.ValidateAsync(new User("alice@example.com", 21));

if (!result.IsSuccess)
{
    foreach (var error in result.Errors)
        Console.WriteLine($"{error.Path}: {error.Message}");
}

public sealed record User(string Email, int Age);
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
