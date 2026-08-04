# Zeta

[![GitHub stars](https://img.shields.io/github/stars/Sonberg/zeta.svg?style=social)](https://github.com/Sonberg/zeta/stargazers)

[![codecov](https://codecov.io/gh/Sonberg/zeta/branch/main/graph/badge.svg)](https://codecov.io/gh/Sonberg/zeta) [![NuGet](https://img.shields.io/nuget/v/Zeta.svg)](https://www.nuget.org/packages/Zeta) [![NuGet Downloads](https://img.shields.io/nuget/dt/Zeta.svg)](https://www.nuget.org/packages/Zeta)
 [![Build](https://github.com/sonberg/zeta/actions/workflows/publish.yml/badge.svg)](https://github.com/Sonberg/zeta/actions) [![Build](https://github.com/sonberg/zeta/actions/workflows/ci.yml/badge.svg)](https://github.com/Sonberg/zeta/actions) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A composable, type-safe, async-first validation framework for .NET inspired by [Zod](https://zod.dev/).

**📖 Full documentation: https://sonberg.github.io/zeta/**

## Basic example

```csharp
var userSchema = Z.Schema<User>()
    .Property(u => u.Email, s => s.Email())
    .Property(u => u.Age, s => s.Min(18));

var result = await userSchema.ValidateAsync(user);

if (!result.IsSuccess)
{
    foreach (var error in result.Errors)
        Console.WriteLine($"{error.PathString}: {error.Message}");
}
```

Use `.Using<TContext>(factory)` to load data asynchronously before validation runs:

```csharp
var createUserSchema = Z.Schema<CreateUserRequest>()
    .Using<CreateUserContext>(async (input, sp, ct) =>
    {
        var repo = sp.GetRequiredService<IUserRepository>();
        return new CreateUserContext(await repo.EmailExistsAsync(input.Email, ct));
    })
    .Property(x => x.Email, s => s.Email())
    .Refine((x, ctx) => !ctx.EmailExists, "Email already exists", "email_exists");
```

## Features

- **Schema-first** — define validation as reusable schema objects
- **Async by default** — every rule can be async, no separate sync path
- **Composable** — schemas are values that can be reused and combined
- **Immutable fluent API** — every call returns a new schema instance, so reuse is safe
- **Path-aware errors** — errors carry a JSONPath location (`$.user.address.street`, `$[0]`)
- **Result pattern** — invalid input is a returned value, not a thrown exception

## Installation

```bash
dotnet add package Zeta
```

Requires .NET 6 or later.

### Which package should I use?

- `Zeta` — core validation for all app types (console, class library, worker, Blazor, MAUI)
- `Zeta.AspNetCore` — ASP.NET Core integration (Minimal APIs, Controllers, endpoint filters)
- `Zeta.FastEndpoints` — [FastEndpoints](https://fast-endpoints.com) integration

## Documentation

| Topic | Link |
|---|---|
| Getting started | https://sonberg.github.io/zeta/getting-started |
| Schema types | https://sonberg.github.io/zeta/schemas |
| Validator reference | https://sonberg.github.io/zeta/validators |
| Results and errors | https://sonberg.github.io/zeta/results |
| Collections | https://sonberg.github.io/zeta/collections |
| Context-aware validation | https://sonberg.github.io/zeta/validation-run |
| Custom rules | https://sonberg.github.io/zeta/custom-rules |
| Validation paths | https://sonberg.github.io/zeta/paths |
| Testing | https://sonberg.github.io/zeta/testing |
| ASP.NET Core | https://sonberg.github.io/zeta/aspnetcore |
| FastEndpoints | https://sonberg.github.io/zeta/fastendpoints |
| Changelog | https://sonberg.github.io/zeta/changelog |

## License

MIT
