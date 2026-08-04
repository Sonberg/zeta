# ASP.NET Core

`Zeta.AspNetCore` wires schemas into Minimal APIs and Controllers. Requires .NET 8 or later.

```bash
dotnet add package Zeta.AspNetCore
```

## Setup

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddZeta();

var app = builder.Build();
```

`AddZeta()` registers `IZetaValidator` and the machinery the endpoint filters need to resolve
services for [context factories](/validation-run).

## Minimal APIs

`.WithValidation(schema)` adds an endpoint filter that runs before your handler. If validation
fails, the request short-circuits — **your handler never runs with invalid input**, so it doesn't
need a guard clause:

```csharp
var createUserSchema = Z.Schema<CreateUserRequest>()
    .Property(x => x.Email, s => s.Email())
    .Property(x => x.Name, s => s.MinLength(2));

app.MapPost("/users", (CreateUserRequest request) => Results.Ok(request))
    .WithValidation(createUserSchema);
```

A failure returns `400 Bad Request` as `ValidationProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "$.email": ["Invalid email format"],
    "$.name": ["Must be at least 2 characters"]
  }
}
```

### Context-aware endpoints

Give the schema a factory with `.Using<TContext>(factory)` and the filter resolves it from the
request's DI scope:

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

The factory runs once per request, before any rule. Route values and headers are reachable through
the service provider if you need them.

::: warning A throwing factory is a 500, not a 400
Exceptions from a context factory propagate — they surface as a server error, not a validation
failure. For an expected condition ("customer not found"), return a context that carries the fact
and fail it with `.Refine()`.
:::

## Controllers

Inject `IZetaValidator` and validate explicitly:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private static readonly ISchema<CreateUserRequest> Schema =
        Z.Schema<CreateUserRequest>()
            .Property(x => x.Email, s => s.Email())
            .Property(x => x.Name, s => s.MinLength(2));

    private readonly IZetaValidator _validator;

    public UsersController(IZetaValidator validator) => _validator = validator;

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        var result = await _validator.ValidateAsync(request, Schema);

        return result.ToActionResult(valid => Ok(valid));
    }
}
```

Keeping the schema in a `static readonly` field builds it once for the lifetime of the process
rather than on every request. Schemas are immutable, so this is safe.

### Result extensions

| Extension | Returns |
|---|---|
| `result.ToActionResult()` | `Ok(value)` or `BadRequest(problem)` |
| `result.ToActionResult(v => …)` | Your result on success, `BadRequest(problem)` on failure |
| `result.ToResult()` | `Results.Ok(value)` or `Results.ValidationProblem(…)` |
| `result.ToResult(v => …)` | Your result on success, `Results.ValidationProblem(…)` on failure |

```csharp
// Controllers
return result.ToActionResult(user => CreatedAtAction(nameof(Get), new { user.Id }, user));

// Minimal APIs
return result.ToResult(user => Results.Created($"/users/{user.Id}", user));
```

## Configuring the validation run

`ValidateAsync` takes an optional builder for the run — cancellation, time, and path formatting:

```csharp
var result = await _validator.ValidateAsync(
    request,
    Schema,
    b => b.WithCancellation(ct).WithTimeProvider(TimeProvider.System));
```

Supplying a `TimeProvider` is what makes date and time rules (`.Past()`, `.MinAge()`, …)
deterministic under test — see [Testing](/testing).

### Path formatting

By default, error paths are camel-cased: `$.email`. `ValidationRunBuilder` infers naming from your
`JsonOptions`, so paths line up with the payload the client actually sent. Override it per call when
you need something else:

```csharp
var result = await _validator.ValidateAsync(
    request,
    Schema,
    b => b.WithPathFormatting(new PathFormattingOptions
    {
        PropertyNameFormatter = static n => n,                        // keep PascalCase
        DictionaryKeyFormatter = static k => k.ToString() ?? string.Empty
    }));
```

More detail in [Validation paths](/paths).

## Notes

- `WithValidation<T, TContext>(...)` expects a context-aware schema, normally one with a factory
  configured via `.Using<TContext>(factory)`.
- Blazor and MAUI apps validating forms without ASP.NET Core endpoints need only the core `Zeta`
  package.
- Using FastEndpoints instead? See [FastEndpoints](/fastendpoints).

The [`Zeta.Sample.Api`](https://github.com/Sonberg/zeta/tree/main/samples/Zeta.Sample.Api) project
is a runnable version of everything on this page.
