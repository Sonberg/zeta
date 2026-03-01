# Zeta.FastEndpoints

[![NuGet](https://img.shields.io/nuget/v/Zeta.FastEndpoints.svg)](https://www.nuget.org/packages/Zeta.FastEndpoints)

[FastEndpoints](https://fast-endpoints.com) integration package for Zeta.

Use this package when you want Zeta schema validation wired into the FastEndpoints pre-processor pipeline.

## Why this exists

This package lets you use Zeta schemas instead of FluentValidation validators while keeping FastEndpoints' built-in validation flow and error responses.

## Installation

```bash
dotnet add package Zeta.FastEndpoints
```

## Setup

No Zeta-specific service registration is required. Schemas are passed directly to endpoints.

Any services used by `.Using<TContext>()` must be registered in the normal ASP.NET Core DI container.

## Usage

### Option 1 — `ZetaEndpoint<TRequest>` base class

Extend `ZetaEndpoint<TRequest>` instead of `Endpoint<TRequest>` and call `Validate(schema)` inside `Configure()`.

`Validate(schema)` registers a `ZetaPreProcessor<TRequest>` under the hood — it is equivalent to `PreProcessors(new ZetaPreProcessor<TRequest>(schema))`.

```csharp
public class RegisterEndpoint : ZetaEndpoint<RegisterRequest>
{
    private static readonly ISchema<RegisterRequest> Schema =
        Z.Schema<RegisterRequest>()
            .Property(r => r.Email, s => s.Email())
            .Property(r => r.Password, s => s.MinLength(8))
            .Property(r => r.Age, s => s.Min(18).Max(120));

    public override void Configure()
    {
        Post("/api/users/register");
        AllowAnonymous();
        Validate(Schema);
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        await SendOkAsync(ct);
    }
}
```

A `ZetaEndpoint<TRequest, TResponse>` overload is also available for endpoints with a typed response.

`Z.Schema<T>()` / `.Property()` are the preferred names. `Z.Object<T>()` / `.Field()` are supported aliases.

### Option 2 — `ZetaPreProcessor<TRequest>` directly

If you need to keep your own base class, register the pre-processor explicitly:

```csharp
public class RegisterEndpoint : Endpoint<RegisterRequest>
{
    private static readonly ISchema<RegisterRequest> Schema =
        Z.Schema<RegisterRequest>()
            .Property(r => r.Email, s => s.Email())
            .Property(r => r.Password, s => s.MinLength(8))
            .Property(r => r.Age, s => s.Min(18).Max(120));

    public override void Configure()
    {
        Post("/api/users/register");
        AllowAnonymous();
        PreProcessors(new ZetaPreProcessor<RegisterRequest>(Schema));
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        await HttpContext.Response.SendOkAsync(ct);
    }
}
```

## Context-Aware Validation

Use `.Using<TContext>(factory)` to load async data before validation runs (e.g. database lookups). The factory receives the request and `IServiceProvider`, so no additional setup is needed.

Context-aware schemas implement `ISchema<T>` directly — the same `ZetaPreProcessor<TRequest>` (or `Validate()` call) handles both cases:

```csharp
public record UserContext(bool EmailExists);

public class RegisterEndpoint : ZetaEndpoint<RegisterRequest>
{
    private static readonly ISchema<RegisterRequest> Schema =
        Z.Schema<RegisterRequest>()
            .Using<UserContext>(async (req, sp, ct) =>
            {
                var repo = sp.GetRequiredService<IUserRepository>();
                return new UserContext(await repo.EmailExistsAsync(req.Email, ct));
            })
            .Property(r => r.Email, s => s.Email())
            .Property(r => r.Password, s => s.MinLength(8))
            .Property(r => r.Age, s => s.Min(18).Max(120))
            .Refine((r, ctx) => !ctx.EmailExists, "Email is already registered", "email_taken");

    public override void Configure()
    {
        Post("/api/users/register");
        AllowAnonymous();
        Validate(Schema);
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        await SendOkAsync(ct);
    }
}
```

## Error Response

Validation failures return `400 Bad Request` in FastEndpoints' standard format:

```json
{
  "statusCode": 400,
  "message": "One or more errors occurred!",
  "errors": {
    "$.email": ["Invalid email format"],
    "$.password": ["Must be at least 8 characters"]
  }
}
```

`ValidationError.Code` is mapped to `ValidationFailure.ErrorCode`. Error codes are not included in the default FastEndpoints response body, but are available when customising error response serialisation.

## Notes

- `Validate(schema)` registers a `ZetaPreProcessor<TRequest>` under the hood.
- Validation short-circuits with a 400 before the handler runs if validation fails.
- Schemas are typically static. The `.Using<TContext>()` factory runs per-request — route params or headers can be accessed there if needed.
- Context factory exceptions propagate as HTTP 500. For expected failures (e.g. entity not found), return a context value and fail with `.Refine(...)` instead.
- For Minimal APIs and Controllers, use [`Zeta.AspNetCore`](../Zeta.AspNetCore/README.md) instead.

## Source and Samples

- Core package docs: [`../Zeta/README.md`](../Zeta/README.md)
- FastEndpoints sample: [`../../samples/Zeta.Sample.FastEndpoints.Api`](../../samples/Zeta.Sample.FastEndpoints.Api)
