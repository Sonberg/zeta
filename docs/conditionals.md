# Conditionals and polymorphism

`.If()` applies rules only when a predicate holds. It's available on every schema type — value,
object, collection and dictionary — and it nests.

## Guarding rules

```csharp
Z.Int()
    .Min(0)
    .If(v => v >= 18, s => s.Max(65));
```

The inner `.Max(65)` only runs when the value is at least 18. Rules outside the guard always run.

## Conditional properties

On an object schema, `.If()` takes a predicate over the whole object and a builder that can add
properties and rules:

```csharp
Z.Schema<Order>()
    .Property(o => o.PaymentMethod, s => s.NotEmpty())
    .If(o => o.PaymentMethod == "card", s => s
        .Property(o => o.CardNumber, n => n.MinLength(16))
        .Property(o => o.Cvv, n => n.Length(3)))
    .If(o => o.PaymentMethod == "bank", s => s
        .Property(o => o.BankAccount, n => n.MinLength(10)));
```

This is the idiomatic way to model "required only when…". Both branches are declared up front, so
the schema still documents the full shape of a valid order.

## Nesting

Guards nest to whatever depth you need:

```csharp
Z.Int()
    .If(v => v >= 0, s => s
        .If(v => v >= 18, inner => inner.Max(100)));
```

Each level's predicate is evaluated against the same value; an outer guard that fails skips
everything inside it.

## Context-aware conditionals

Once a schema is promoted with `.Using<TContext>()`, `.If()` accepts a predicate over the value and
the context:

```csharp
Z.String()
    .Using<SecurityContext>()
    .If((v, ctx) => ctx.RequireStrongPassword, s => s.MinLength(12));
```

Value-only predicates keep working after promotion, so you can mix both styles in one schema. See
[Context-aware validation](/validation-run).

## Polymorphic validation

For a base type or interface, use `.If()` with a **branch schema** — a complete schema for the
derived type:

```csharp
var dogSchema = Z.Schema<Dog>()
    .Property(x => x.BarkVolume, x => x.Min(0).Max(100));

var animalSchema = Z.Schema<IAnimal>()
    .If(x => x is Dog, dogSchema)
    .If(x => x is Cat, Z.Schema<Cat>()
        .Property(x => x.ClawSharpness, x => x.Min(1).Max(10)));
```

Each branch runs only for its matching runtime type. A value that matches no branch simply passes
the conditional stage — add a `.Refine()` if an unrecognised subtype should itself be an error:

```csharp
Z.Schema<IAnimal>()
    .If(x => x is Dog, dogSchema)
    .If(x => x is Cat, catSchema)
    .Refine(x => x is Dog or Cat, "Unsupported animal type", "unsupported_type");
```

### Type assertions

`.As<TDerived>()` asserts the runtime type directly, failing with `type_mismatch` when it doesn't
match:

```csharp
Z.Schema<IAnimal>().As<Dog>();
```

Prefer branch schemas. `.As<T>()` says "this must be a Dog", which is a stronger and less composable
claim than "validate it as a Dog if it is one" — and it can't express a second valid subtype.

## Ordering

Within an object schema, conditionals run **after** properties and type assertions but **before**
object-level rules:

1. Properties
2. Type assertions (`.As<T>()`)
3. Conditionals (`.If()`)
4. Rules (`.Refine()`)

Every stage runs regardless of earlier failures — a failed property doesn't prevent a conditional
from being evaluated, so you get the complete error set in one pass.
