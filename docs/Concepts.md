# Core concepts

Three things trip up newcomers (especially from FluentValidation). Read these once.

## 1. Schemas are values, not services

A schema is an immutable value — like a compiled regex. Every fluent call returns a **new** schema; nothing mutates. So:

- Hold schemas as `static readonly` (or static get-only) fields. Build once, reuse forever.
- Schemas are **thread-safe** — the same instance validates concurrent requests safely.
- **Don't** register `ISchema<T>` in DI, don't scan assemblies for "validators", don't `new` a schema per request. There is no validator *object* to wire up.
- Runtime dependencies (repositories, the current user, clocks) are resolved **at validation time** inside a context factory (`.Using<TContext>(factory)`), not baked into the schema.

```csharp
public static class Schemas
{
    public static readonly ISchema<CreateUser> CreateUser =
        Z.Schema<CreateUser>()
            .Property(u => u.Email, s => s.Email())
            .Property(u => u.Age, s => s.Min(18));
}
```

If you're writing an `AbstractValidator<T>` subclass, stop — see [MigratingFromFluentValidation.md](MigratingFromFluentValidation.md).

## 2. No short-circuit — every stage runs, errors aggregate

Zeta runs **all** validation stages and returns **every** error it finds. It never stops at the first failure. This is deliberate (one round-trip surfaces the whole form), but it differs from FluentValidation's `CascadeMode.Stop` / `DependentRules`.

Consequence: a rule cannot assume an earlier rule "passed". If a later rule depends on a value being present/valid, guard it — otherwise it runs against the raw/default value and may emit a misleading secondary error.

```csharp
// A refinement that only makes sense once the prerequisite holds:
Z.String().NotEmpty()
    .Refine(s => s.Length < 100 || string.IsNullOrEmpty(s), "Too long"); // guarded
```

For prerequisites that require **loaded** data (an entity exists, an id resolves), load it in the context factory and return a context that fails validation — see the `Result<TContext>` factory in [ValidationRun.md](ValidationRun.md) and the migration guide's `DependentRules` row.

## 3. Validation order is a guarantee

For object schemas, stages run in this order, and errors appear in `Result.Errors` in this order:

1. **Properties** (`.Property(...)`)
2. **Type assertions** (`.As<T>()`)
3. **Conditionals** (`.If(...)`)
4. **Rules / refinements** (`.Refine(...)`)

Collection and dictionary schemas run their element/entry rules first, then collection-level rules. No stage is skipped and no error is dropped — order only affects the sequence errors appear in.
