# Glossary

The vocabulary Zeta uses in its API, error messages and these docs.

## Schemas

**Schema**
: A validator built from the `Z` entry point — `Z.String()`, `Z.Schema<T>()`, `Z.Collection<T>()`.
Schemas are immutable values: every fluent call returns a new instance, so they're safe to store
statically and reuse across threads.

**Contextless schema**
: A schema that validates a value on its own and returns `Result<T>`. The interface is `ISchema<T>`.

**Context-aware schema**
: A schema whose rules also receive a `TContext` — external data loaded for this validation. The
interface is `ISchema<T, TContext>`.

::: warning These two are separate interfaces
`ISchema<T, TContext>` does **not** inherit from `ISchema<T>`. That's deliberate: it keeps overload
resolution unambiguous and stops a context-aware schema being silently used where no context will be
supplied. Use `SchemaAdapter<T, TContext>` to bridge between them.
:::

**Context promotion**
: Turning a contextless schema into a context-aware one with `.Using<TContext>()`. Rules,
properties, conditionals and type assertions all transfer — you don't rebuild the schema.

**Context factory**
: The delegate that builds the `TContext` for a run, from the value plus an `IServiceProvider`:
`Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>`. It runs once per validation
run, before any rule.

## Running validation

**Validation run**
: The per-execution record carrying the current path, the cancellation token, the `TimeProvider`,
the service provider and path-formatting options. The type is `ValidationRun` /
`ValidationRun<TData>`.

::: tip "Context" means one specific thing
The word was historically overloaded three ways in this codebase — the execution record, the
`TContext` data, and the factory that builds it. They're now distinct: the execution record is the
**validation run**, and "context" refers to your `TContext` **data**. This is why `ValidationContext`
was renamed to `ValidationRun`.
:::

**Rule**
: One unit of validation logic appended to a schema — `.Min(1)`, `.Email()`, `.Refine(...)`.

**Refinement**
: A custom predicate rule added with `.Refine()`, `.RefineAt()`, or `.RefineEachEntry()`.

**Conditional**
: A branch applied only when a predicate holds, added with `.If(...)`. See
[Conditionals](/conditionals).

**Type assertion**
: Narrowing an object schema to a derived type with `.As<TDerived>()`. Runs as its own stage and
fails with `type_mismatch`.

**Stage**
: One step of a schema's validation. Object schemas run properties → type assertions → conditionals
→ rules; collection and dictionary schemas run their rules first.

**Aggregation**
: Collecting errors from every stage without short-circuiting. This is why a schema with three
broken properties returns three errors rather than the first one.

## Results and errors

**Result**
: The outcome of validation — a success carrying a value, or a failure carrying errors. Never an
exception for ordinary invalid input. See [Results and errors](/results).

**Validation error**
: A single failure, with a `Path`, a `Code`, a `Message`, and the `AttemptedValue`.

**Code**
: The machine-readable identifier for a failure — `min_length`, `email`, `custom_error`. This is
what client code should branch on; messages are for humans and may be reworded.

**Path**
: The JSONPath location of an error, such as `$.items[0].name`. Built from structured segments and
rendered to a string via `PathString`. See [Validation paths](/paths).

**Attempted value**
: The exact value being validated when the error was produced, captured at the point of failure
rather than re-resolved from the path afterwards — so it stays correct for camel-cased paths and
non-string dictionary keys.

## Terms that changed

Older blog posts, answers from language models, and pre-0.1.17 code may use names that no longer
exist:

| Old | Current | Notes |
|---|---|---|
| `Z.Object<T>()` | `Z.Schema<T>()` | Removed in 0.1.17, not aliased |
| `.Field(...)` | `.Property(...)` | Removed in 0.1.17, not aliased |
| `ValidationContext` | `ValidationRun` | Renamed to free up "context" for `TContext` |
| `ValidationContextBuilder` | `ValidationRunBuilder` | Same rename |

If you hit a compiler error on one of the first two, it's this — the old names were removed outright
rather than kept as aliases.
