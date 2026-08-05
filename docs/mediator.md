# Zeta + Mediator Integration

This guide shows how to integrate **Zeta** with the **Mediator pattern** (e.g. [MediatR](https://github.com/jbogard/MediatR)) in a clean, idiomatic, and async-first way.

Each handler owns its schema as a value and validates the request through the injected
`IZetaValidator` before doing any work. Validation returns a `Result<T>`, so there are no
exceptions and no control-flow abuse.

The runnable version of everything below lives in [`samples/Zeta.Sample.MediatR`](https://github.com/Sonberg/zeta/tree/main/samples/Zeta.Sample.MediatR).

---

## Why Zeta fits Mediator well

* **Async-first** – validation and context loading are async by design
* **Schema as values** – schemas live next to their handler
* **Result-based** – no exceptions, no control-flow abuse
* **Context-aware** – async data loading fits the Application layer naturally
* **No ASP.NET dependency** – `IZetaValidator` lives in the core `Zeta` package, so it works in any layer

---

## 1. Register the validator

`IZetaValidator` is the injectable entry point. It carries the `IServiceProvider`, which is what
lets context-aware schemas resolve their factories (see section 3).

```csharp
services.AddScoped<IZetaValidator, ZetaValidator>(); // core Zeta — no ASP.NET reference needed
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
```

---

## 2. A command and its handler

Keep the schema as a `static readonly` field on the handler — it is an immutable value, built once.
Inject `IZetaValidator` and validate at the top of `Handle`.

```csharp
public sealed record CreateUserCommand(string Email, int Age) : IRequest<Result<string>>;

public sealed class CreateUserHandler(IZetaValidator validator) : IRequestHandler<CreateUserCommand, Result<string>>
{
    private static readonly ISchema<CreateUserCommand> Schema = Z
        .Schema<CreateUserCommand>()
        .Property(x => x.Email, Z.String().Email())
        .Property(x => x.Age, Z.Int().Min(18));

    public async Task<Result<string>> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, Schema, ct);

        return validation.IsSuccess
            ? Result<string>.Success($"created user {cmd.Email}")
            : Result<string>.Failure(validation.Errors);
    }
}
```

The schema is strongly typed, reusable, testable, and colocated with the handler.

---

## 3. Context-aware validation (async data loading)

Zeta can load data *before* validation runs via `.Using<TContext>(factory)`. The factory receives the
service provider — which is why validation must go through `IZetaValidator` (it supplies the SP that a
bare `Schema.ValidateAsync(cmd)` call would not).

Use the **validation-aware** factory overload — a `Result<TContext>` factory — to turn a missing
prerequisite (e.g. "order not found") into a normal aggregated validation error instead of a 500 or an
ad-hoc `Exists` flag on the context.

```csharp
public sealed record OrderContext(bool IsOpen);

public sealed record UpdateOrderCommand(int OrderId, string Note) : IRequest<Result<string>>;

public sealed class UpdateOrderHandler(IZetaValidator validator) : IRequestHandler<UpdateOrderCommand, Result<string>>
{
    private static readonly ISchema<UpdateOrderCommand> Schema = Z
        .Schema<UpdateOrderCommand>()
        .Property(x => x.Note, Z.String().NotEmpty().MaxLength(200))
        .Using<OrderContext>(async (cmd, sp, ct) =>
        {
            var order = await sp.GetRequiredService<IOrderRepo>().FindAsync(cmd.OrderId, ct);

            return order is null
                ? Result<OrderContext>.Failure(new ValidationError("$.orderId", "not_found", $"Order {cmd.OrderId} not found"))
                : Result<OrderContext>.Success(new OrderContext(order.IsOpen));
        })
        .Refine((_, ctx) => ctx.IsOpen, "Order is not open", "order_closed");

    public async Task<Result<string>> Handle(UpdateOrderCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, Schema, ct);

        return validation.IsSuccess
            ? Result<string>.Success($"updated order {cmd.OrderId}")
            : Result<string>.Failure(validation.Errors);
    }
}
```

A missing order fails validation with a `$.orderId` / `not_found` error; a closed order fails the
`order_closed` refine rule. Neither throws.

---

## Summary

* Schemas are `static readonly` values on their handler
* Handlers inject `IZetaValidator` and validate at the top of `Handle`
* Failures come back as `Result<T>.Failure(errors)` — no exceptions
* Async context (and "entity not found") loads via `.Using<TContext>(...)` factories

This scales cleanly from simple commands to context-aware validation.

---

Happy validating ✨
