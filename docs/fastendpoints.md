# FastEndpoints

`Zeta.FastEndpoints` runs Zeta schemas inside the [FastEndpoints](https://fast-endpoints.com)
pre-processor pipeline, so you can use schemas in place of FluentValidation validators while keeping
FastEndpoints' own validation flow and error responses.

```bash
dotnet add package Zeta.FastEndpoints
```

Requires .NET 8+ and FastEndpoints 7.2.0 or later.

## Setup

No Zeta-specific service registration is needed — schemas are passed straight to endpoints. Services
used by a `.Using<TContext>()` factory must be registered in the normal DI container.

## Three ways to wire it up

### Option 1 — the `ZetaEndpoint<TRequest>` base class

Extend `ZetaEndpoint<TRequest>` instead of `Endpoint<TRequest>`, then call `Validate(schema)` in
`Configure()`:

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
        => await SendOkAsync(ct);
}
```

`Validate(schema)` registers a `ZetaPreProcessor<TRequest>` for you. A
`ZetaEndpoint<TRequest, TResponse>` overload exists for endpoints with a typed response.

### Option 2 — the pre-processor directly

Keep your own base class and register the pre-processor explicitly:

```csharp
public class RegisterEndpoint : Endpoint<RegisterRequest>
{
    private static readonly ISchema<RegisterRequest> Schema = /* … */;

    public override void Configure()
    {
        Post("/api/users/register");
        AllowAnonymous();
        PreProcessors(new ZetaPreProcessor<RegisterRequest>(Schema));
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
        => await HttpContext.Response.SendOkAsync(ct);
}
```

### Option 3 — global configurator, zero per-endpoint boilerplate

Turn on convention-based discovery once in `Program.cs`:

```csharp
app.UseFastEndpoints(c =>
{
    c.Endpoints.Configurator = ep => ep.UseZetaValidation();
});
```

Now any endpoint with a static `ISchema<TRequest>` field gets validation automatically:

```csharp
public class CreateOrderEndpoint : Endpoint<CreateOrderRequest>
{
    private static readonly ISchema<CreateOrderRequest> Schema =
        Z.Schema<CreateOrderRequest>()
            .Property(r => r.ProductId, s => s.NotEmpty())
            .Property(r => r.Quantity, s => s.Min(1));

    public override void Configure()
    {
        Post("/api/orders");
        AllowAnonymous();
        // No Validate() call — the configurator finds Schema
    }

    public override async Task HandleAsync(CreateOrderRequest req, CancellationToken ct)
        => await HttpContext.Response.SendOkAsync(ct);
}
```

**The convention:** the first static field of type `ISchema<TRequest>` on the endpoint class — or on
a base class, up to but not including the FastEndpoints `Endpoint<T>` base — is used. Fields on
derived classes win over base-class fields.

**Safe to adopt incrementally.** Endpoints with no `ISchema<TRequest>` field are skipped silently,
and an endpoint that already calls `Validate(Schema)` still validates only once — the pre-processors
check `Response.HasStarted` and bail if an earlier one already sent the error response.

## Cross-field rules

`.RefineAt()` puts a cross-field error on a specific property:

```csharp
var schema = Z.Schema<DateRangeRequest>()
    .Property(r => r.StartDate, s => s.Future())
    .Property(r => r.EndDate, s => s.Future())
    .RefineAt(r => r.EndDate,
        r => r.EndDate > r.StartDate,
        "End date must be after start date");   // reported at $.endDate
```

For rules that need async data, load it in a context factory:

```csharp
var schema = Z.Schema<CheckoutRequest>()
    .Using<InventoryContext>(async (req, sp, ct) =>
    {
        var svc = sp.GetRequiredService<IInventoryService>();
        return new InventoryContext(await svc.IsAvailableAsync(req.ProductId, req.Quantity, ct));
    })
    .Property(r => r.ProductId, s => s.NotEmpty())
    .Property(r => r.Quantity, s => s.Min(1))
    .Refine((r, ctx) => ctx.IsAvailable, "Product not available in requested quantity");
```

Context-aware schemas implement `ISchema<T>` directly, so the same `ZetaPreProcessor<TRequest>` and
`Validate()` call handle both flavours — nothing changes at the endpoint.

## Error response

Failures return `400 Bad Request` in FastEndpoints' standard shape:

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

`ValidationError.Code` maps to `ValidationFailure.ErrorCode`. Codes aren't in the default response
body, but they're available if you customise error serialisation.

## Migrating from Zeta.AspNetCore

| Before (`Zeta.AspNetCore`) | After (`Zeta.FastEndpoints`) |
|---|---|
| `builder.Services.AddZeta()` | No service registration needed |
| `IZetaValidator` constructor injection | Remove from constructor |
| Manual `validator.ValidateAsync(req, …)` | Remove — the pre-processor handles it |
| `.WithValidation(schema)` on `MapPost(...)` | `Validate(Schema)` in `Configure()` |
| `ValidationProblem` in a `Results<>` return | Remove — validation short-circuits first |

**Before:**

```csharp
app.MapPost("/api/users", async (IZetaValidator validator, UserRequest req, CancellationToken ct) =>
{
    var result = await validator.ValidateAsync(req, schema, ct);
    if (!result.IsSuccess) return Results.ValidationProblem(/* … */);
    return Results.Ok();
}).WithValidation(schema);
```

**After:**

```csharp
public class CreateUserEndpoint : ZetaEndpoint<UserRequest>
{
    private static readonly ISchema<UserRequest> Schema = /* … */;

    public override void Configure()
    {
        Post("/api/users");
        Validate(Schema);
    }

    public override async Task HandleAsync(UserRequest req, CancellationToken ct)
        => await SendOkAsync(ct);   // only reached when validation passed
}
```

## Notes

- Validation short-circuits with a 400 before the handler runs.
- Schemas are normally static; the `.Using<TContext>()` factory still runs per request, so route
  params and headers can be read there.
- Context factory exceptions become HTTP 500. For expected failures, return a context and fail it
  with `.Refine()`.
- For Minimal APIs and Controllers, use [`Zeta.AspNetCore`](/aspnetcore) instead.

The [`Zeta.Sample.FastEndpoints.Api`](https://github.com/Sonberg/zeta/tree/main/samples/Zeta.Sample.FastEndpoints.Api)
project is a runnable version of this page.
