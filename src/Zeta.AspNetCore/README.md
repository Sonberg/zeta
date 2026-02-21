# Zeta.AspNetCore

[![NuGet](https://img.shields.io/nuget/v/Zeta.AspNetCore.svg)](https://www.nuget.org/packages/Zeta.AspNetCore)

ASP.NET Core integration package for Zeta.

Use this package when you want validation integrated with:
- Minimal APIs (`WithValidation(...)`)
- MVC Controllers (`IZetaValidator`)
- Validation context builder utilities

## Installation

```bash
dotnet add package Zeta.AspNetCore
```

## Setup

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddZeta();
```

## Minimal API Example

```csharp
var createUserSchema = Z.Schema<CreateUserRequest>()
    .Property(x => x.Email, s => s.Email())
    .Property(x => x.Name, s => s.MinLength(2));

app.MapPost("/users", (CreateUserRequest request) => Results.Ok(request))
    .WithValidation(createUserSchema);
```

### Context-aware Minimal API Example

Use `.Using<TContext>(factory)` on the schema so the validation filter can resolve context data:

```csharp
var schema = Z.Schema<CreateOrderRequest>()
    .Using<OrderContext>(async (value, sp, ct) =>
    {
        var permissions = sp.GetRequiredService<IPermissionsService>();
        var allowed = await permissions.CanCreateOrderAsync(value.CustomerId, ct);
        return new OrderContext(allowed);
    })
    .Property(x => x.CustomerId, s => s.NotEmpty())
    .Refine((x, ctx) => ctx.CanCreateOrder, "No permission to create order");

app.MapPost("/orders", (CreateOrderRequest request) => Results.Ok(request))
    .WithValidation(schema);
```

## Controller Example

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IZetaValidator _validator;

    public UsersController(IZetaValidator validator)
    {
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        var schema = Z.Schema<CreateUserRequest>()
            .Property(x => x.Email, s => s.Email())
            .Property(x => x.Name, s => s.MinLength(2));

        var result = await _validator.ValidateAsync(request, schema);
        return result.ToActionResult(valid => Ok(valid));
    }
}
```

## Context Configuration

```csharp
var result = await _validator.ValidateAsync(
    request,
    schema,
    b => b.WithCancellation(ct).WithTimeProvider(TimeProvider.System));
```

### Path Formatting

You can override path rendering per call:

```csharp
var result = await _validator.ValidateAsync(
    request,
    schema,
    b => b.WithPathFormatting(new PathFormattingOptions
    {
        PropertyNameFormatter = static n => n,
        DictionaryKeyFormatter = static k => k.ToString() ?? string.Empty
    }));
```

If not overridden, `ValidationContextBuilder` can infer naming policies from `JsonOptions`.

## Notes
- For Blazor and MAUI form validation without ASP.NET Core endpoints, `Zeta` alone is enough.
- `WithValidation<T, TContext>(...)` expects a context-aware schema, typically with a built-in factory configured via `.Using<TContext>(factory)`.

## Source and Samples

- Core package docs: [`../Zeta/README.md`](../Zeta/README.md)
- Path formatting and structured paths: [`../../docs/Paths.md`](../../docs/Paths.md)
- API sample: [`../../samples/Zeta.Sample.Api`](../../samples/Zeta.Sample.Api)
