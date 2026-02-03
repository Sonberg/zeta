# RFC 004

## Type Assertions via `As<T>`

---

## Motivation

Zeta supports powerful conditional validation via `If` and explicit branching via `Switch`. However, conditional logic alone is not sufficient to provide **runtime type guarantees** when validating polymorphic or interface-based models.

Today, users may write conditions that *appear* to narrow a type, but without an explicit runtime check, those guarantees are soft and can fail silently.

This RFC introduces **`As<T>`** as a first-class concept:

> A *type assertion that participates in validation*.

---

## Problem Statement

Consider the following validation:

```csharp
.Switch(s =>
    s.Case(
        animal => animal is Dog,
        schema => schema
            .Field(a => a.Name, v => v.MinLength(1))
    )
)
```

Although the condition checks `animal is Dog`, the schema itself is still typed as `IAnimal`. Any assumption that the value *must* be a `Dog` is implicit and unenforced.

This creates several issues:

* Type guarantees are **not validated**
* Bugs can be silently ignored if conditions drift
* APIs may *look* type-safe while not being so

---

## Proposal

Introduce `As<T>()` as a **type assertion operator** that:

* Performs a runtime type check
* Fails validation if the value is not of type `T`
* Narrows the schema to `Schema<T>` on success

`As<T>` is **not casting**. It is validation.

---

## Semantics

```text
If value is T    → continue validation as Schema<T>
If value is not T → emit validation error
```

There is no branching and no silent skipping.

---

## Basic Usage

```csharp
Z.Value<IAnimal>()
    .As<Dog>()
    .Field(d => d.BarkVolume, v => v.Min(0).Max(100));
```

If the runtime value is not a `Dog`, validation fails.

---

## Usage with `Switch`

```csharp
Z.Collection<IAnimal>()
    .ForEach(x => x.Switch(s =>
        s.Case(
            animal => animal is Dog,
            schema => schema
                .As<Dog>()
                .Field(d => d.BarkVolume, v => v.Min(0).Max(100))
        )
        .Case(
            animal => animal is Cat,
            schema => schema
                .As<Cat>()
                .Field(c => c.ClawSharpness, v => v.Min(0).Max(10))
        )
    ));
```

Here:

* `Switch` controls **which branch applies**
* `As<T>` enforces **what the value must be**

If the condition and the asserted type disagree, validation fails explicitly.

---

## Error Semantics

When `As<T>` fails, it produces a validation error:

* Error code: `type_mismatch`
* Message example: `Expected value to be of type 'Dog' but was 'Cat'`
* Error path: current schema path

This error is **deterministic** and **non-recoverable** within the current branch.

---

## Context Awareness

`As<T>` may be used in both contextless and context-aware schemas.

```csharp
Z.Value<IAnimal>()
    .WithContext<MyContext>()
    .As<Dog>()
    .If((dog, ctx) => ctx.RequiresTraining, s =>
        s.Field(d => d.TrainingLevel, v => v.Min(1))
    );
```

Context does not affect the type check itself.

---

## Non-goals

This RFC explicitly does **not** introduce:

* Implicit type narrowing
* Silent skipping on mismatch
* Branching behavior
* Compile-time casting guarantees

All type refinement must be explicit and validated.

---

## Relationship to Other RFCs

| RFC     | Relationship                                                 |
| ------- | ------------------------------------------------------------ |
| RFC 002 | `If` provides conditional augmentation, not type guarantees  |
| RFC 003 | `Switch` provides branching, not type enforcement            |
| RFC 004 | `As<T>` provides runtime type assertion and schema narrowing |

Each concept has a single, clear responsibility.

---

## Design Rationale

* Keeps type guarantees **honest**
* Avoids APIs that *look* safer than they are
* Improves predictability for users and tooling
* Enables robust polymorphic validation

`As<T>` answers the question:

> *"Is this value actually of this type?"*

---

## Summary

* `As<T>` is a **failable type assertion**
* It narrows the schema on success
* It produces a validation error on failure
* It composes naturally with `If` and `Switch`

This completes the conditional validation model in Zeta:

* `If` → conditional rules
* `Switch` → conditional schemas
* `As<T>` → conditional types
