# RFC 002

## Improve mental model around Conditional Validation

---

## Motivation

Conditional validation is one of the most powerful parts of Zeta, but also one of the easiest places to lose clarity. The goal of this RFC is to:

* Make conditional validation **easy to reason about**
* Keep the API **composable and linear**
* Clearly separate **guard-style conditions** from **branching logic**
* Improve **LLM-friendliness** and predictability

---

## Rename `When` → `If`

Rename `.When(...)` to `.If(...)` to better align with common mental models and language.

* `If` reads as a *guard*
* It communicates intent clearly: *"apply these rules if the condition holds"*

`If` can be applied to:

* Object schemas
* Collection schemas
* Value schemas

---

## `If` — Guard-style conditional validation

### Core principle

> `If` **does not branch the schema**.
> It conditionally **augments** the existing schema.

There is **no `elseBranch`**.

This keeps schemas:

* Linear
* Composable
* Easy to stack and refactor

---

### Semantics

```text
If condition is true  → apply the provided rules
If condition is false → do nothing
```

Multiple `If` calls can be chained to express more complex logic.

---

### Contextless `If`

```csharp
Z.Int()
    .If(v => v >= 18, s => s.Max(100))
    .If(v => v < 18,  s => s.Min(0).Max(17));
```

```csharp
Z.Object<User>()
    .If(
        u => u.HasEmail,
        s => s.Property(x => x.Email).Email()
    )
    .If(
        u => !u.HasEmail,
        s => s.Property(x => x.Email).Nullable()
    );
```

---

### Context-aware `If`

```csharp
Z.String()
    .WithContext<MyContext>()
    .If(
        (value, ctx) => ctx.IsSpecialUser,
        s => s.MinLength(10)
    )
    .If(
        (value, ctx) => !ctx.IsSpecialUser,
        s => s.MinLength(5)
    );
```

---

### `If` on collections

```csharp
Z.Collection<OrderItem>()
    .If(
        items => items.Count > 0,
        s => s.MinLength(1)
    );
```

---

### Design rationale

* Avoids hidden branching
* Encourages explicit conditions
* Plays well with fluent chaining
* Easier for tooling and LLMs to reason about

`If` answers the question:

> *"Under what condition should these additional rules apply?"*

---

## `Switch` — Explicit branching

When validation logic is **mutually exclusive**, use `.Switch(...)`.

`Switch` is the **only API that introduces branching**.

---

### Goals of `Switch`

* Express multi-branch conditional logic
* Support contextless and context-aware conditions
* Support pattern matching
* Allow schema specialization per branch

---

### Example: value-based branching

```csharp
Z.Collection<IAnimal>()
    .Each(x => x.Switch(s =>
        s.Case(
            condition: (animal, ctx) => animal.Type == "Dog",
            branch: dog => dog
                .Field(a => a.BarkVolume, v => v.Min(0).Max(100))
        )
        .Case(
            condition: (animal, ctx) => animal.Type == "Cat",
            branch: cat => cat
                .Field(a => a.ClawSharpness, v => v.Min(0).Max(10))
        )
        .Default(
            other => other
                .Field(a => a.Name, v => v.MinLength(1))
        )
    ));
```

---

### Example: pattern-matching with typed schemas

```csharp
Z.Collection<IAnimal>()
    .Each(x => x.Switch(s =>
        s.Case(
            condition: (animal, ctx) => animal is Dog,
            branch: (ContextlessSchema<Dog> dog) => dog
                .Field(a => a.BarkVolume, v => v.Min(0).Max(100))
        )
        .Case(
            condition: (animal, ctx) => animal is Cat,
            branch: (ContextlessSchema<Cat> cat) => cat
                .Field(a => a.ClawSharpness, v => v.Min(0).Max(10))
        )
        .Default(
            (ContextlessSchema<IAnimal> other) => other
                .Field(a => a.Name, v => v.MinLength(1))
        )
    ));
```

---

### Design rationale

* Makes branching **explicit**
* Avoids ambiguity between `If` and `else`
* Maps naturally to pattern matching
* Scales from simple `if/else` to complex decision trees

`Switch` answers the question:

> *"Which schema should apply for this value?"*

---

## Summary

| API      | Purpose                                  |
| -------- | ---------------------------------------- |
| `If`     | Conditional **augmentation** of a schema |
| `Switch` | Conditional **selection** of a schema    |

Key decisions:

* `If` has **no `elseBranch`**
* Branching lives exclusively in `Switch`
* Linear chains over nested conditionals

This separation keeps Zeta powerful, predictable, and easy to reason about.
