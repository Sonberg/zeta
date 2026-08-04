# Migrating from FluentValidation

Zeta and FluentValidation solve the same problem with different shapes. Read [Concepts.md](Concepts.md) first — the biggest mental shift is *schemas are immutable values, not validator services*.

## Mapping table

| FluentValidation | Zeta |
|---|---|
| `class FooValidator : AbstractValidator<Foo>` | `static readonly ISchema<Foo>` built with `Z.Schema<Foo>()` |
| Assembly scanning / `AddValidatorsFromAssembly` | Nothing — hold schemas as static values; no registration |
| `RuleFor(x => x.Name).NotEmpty()` | `.Property(x => x.Name, s => s.NotEmpty())` |
| `MustAsync(...)` (needs a service) | `.Using<TContext>(factory)` + `.Refine((v, ctx) => ...)` / `.RefineAsync(...)` |
| `SetValidator(new AddressValidator())` | `.Property(x => x.Address, addressSchema)` (compose the pre-built schema) |
| `RuleForEach(x => x.Items)` / `ChildRules` | `.Each(...)` on a collection schema |
| `When(...)` / `Unless(...)` | `.If(predicate, schema)` |
| `DependentRules(...)` | Load the prerequisite in the context factory; return `Result<TContext>.Failure(...)` so it fails cleanly. Guard dependent rules with `.If(...)`. |
| `ValidationContext.RootContextData` | Typed `TContext` via `.Using<TContext>` |
| `WithMessage` / `WithErrorCode` | `.WithError(x => x.Message(...).Code(...).Path(...))` after the rule |
| `throw new ValidationException` | Return `Result<T>` and branch on `IsSuccess`; or adapt at the integration boundary |
| `CascadeMode.Stop` | Not available by design — Zeta aggregates all errors, never short-circuits (see below) |

## Two behavioral differences that bite

**1. No cascade / no short-circuit.** FluentValidation can stop at the first failure and chain `DependentRules`. Zeta always runs every stage and aggregates every error. Rules that assumed a prerequisite passed must be guarded (`.If`) or moved into a validation-aware context factory. See [Concepts.md #2](Concepts.md).

**2. Error ordering and path format.** Zeta emits JSONPath-style paths (`$.items[0].name`, camel-cased by default) and a fixed stage order (Properties → `.As()` → `.If()` → `.Refine()`). If you have a stable external error contract, use `.WithError(x => x.Path(...))` to pin paths and `PathFormattingOptions` (via `ValidationRunBuilder.WithPathFormatting`) to control casing/root prefix.

## Prerequisite-loading without secondary errors

The FluentValidation pattern "load entity, then validate it, stop if missing" maps to a **validation-aware context factory**:

```csharp
Z.Schema<UpdateOrder>()
    .Using<OrderContext>(async (req, sp, ct) =>
    {
        var order = await sp.GetRequiredService<IOrderRepo>().FindAsync(req.OrderId, ct);
        return order is null
            ? Result<OrderContext>.Failure(new ValidationError("$.orderId", "not_found", "Order not found"))
            : Result<OrderContext>.Success(new OrderContext(order));
    })
    .Refine((req, ctx) => ctx.Order.Status == OrderStatus.Open, "Order is not open");
```

If the factory fails, that error is aggregated normally (no 500), and you guard downstream rules so they don't dereference missing data.

## Where to put schemas in Clean Architecture

Schemas are dependency-free values, so define them in the **Application** layer next to the request/command they validate. `IZetaValidator` + `ZetaValidator` live in the core `Zeta` package (not `Zeta.AspNetCore`), so the Application layer can execute validation without referencing ASP.NET. Keep only the HTTP result helpers (`ToActionResult`, `WithValidation`, the error-envelope factory) in the web layer.
