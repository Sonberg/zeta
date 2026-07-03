# Ubiquitous Language

Terminology for Zeta's validation model, as used across the codebase and architecture reviews.
Kept consistent with `CONTEXT.md` (which is the shorter canonical index); this file adds aliases,
relationships, and flagged ambiguities.

## Schemas & context

| Term                  | Definition                                                                                          | Aliases to avoid                     |
| --------------------- | --------------------------------------------------------------------------------------------------- | ------------------------------------ |
| **Schema**            | A validator with a small interface and an implementation, built via the `Z` entry point            | Validator, rule set                  |
| **Contextless schema**| A schema that validates a value alone and returns `Result<T>` (`ISchema<T>`)                        | Plain schema, simple schema          |
| **Context-aware schema** | A schema that validates against external `Data` and returns `Result<T, TContext>` (`ISchema<T, TContext>`) | Stateful schema, contextful schema |
| **Context promotion** | Turning a contextless schema into a context-aware one via `.Using<TContext>()`, transferring rules, conditionals, and assertions | Upgrade, conversion                  |
| **Context factory**   | A delegate that builds the `TContext` data during validation from the value + `IServiceProvider`    | Provider, resolver, builder          |
| **Validation run**| The per-validation execution record carrying path, cancellation, time, and service provider         | Execution, environment, state        |

## Validation execution

| Term                   | Definition                                                                                                          | Aliases to avoid                    |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------ | ----------------------------------- |
| **Stage**              | One step of a schema's validation that produces errors for a value (fields, type assertion, conditionals, rules, elements, entries) | Step, phase, check, pass            |
| **Stage order**        | The sequence in which a schema runs its stages, declared per schema as data (`Stages()`), not an implicit call order | Validation order, sequence          |
| **Validation pipeline**| The single runner (`ValidationPipeline.RunAsync`) that executes a schema's stages in order and aggregates all errors | Executor, engine, orchestrator      |
| **Rule**               | A single unit of validation logic appended to a schema (`.Refine`, `.Email`, `.Min`, …)                            | Constraint, check, validator        |
| **Refinement**         | A custom predicate rule attached via `.Refine` / `.RefineAt` / `.RefineEachEntry`                                   | Custom rule, assertion              |
| **Rule engine**        | The per-schema executor that runs a schema's rules in insertion order                                               | Runner, evaluator                   |
| **Rule chain**         | The immutable append-only list backing a rule engine (persistent linked list, lazy insertion-order materialization) | Rule list, node list                |
| **Conditional**        | A branch applied only when a predicate holds, added via `.If(...)`                                                  | Branch, guard, when-clause          |
| **Type assertion**     | A narrowing of an object schema to a derived type via `.As<TDerived>()`, validated as its own stage                 | Cast, downcast, type check          |
| **Aggregation**        | Collecting errors from every stage without short-circuiting                                                         | Accumulation, gathering             |

## Results & errors

| Term                 | Definition                                                                                       | Aliases to avoid          |
| -------------------- | ------------------------------------------------------------------------------------------------ | ------------------------- |
| **Result**           | The discriminated outcome of validation — success with a value, or failure with errors           | Outcome, response, return |
| **Validation error** | A single JSONPath-aware failure: `Path`, `Code`, `Message`, and the captured `AttemptedValue`     | Failure, violation, issue |
| **Attempted value**  | The exact value being validated when an error was produced, captured at the point of failure      | Bad value, input          |
| **Path**             | The JSONPath location of an error (`$.items[0].name`), built from structured segments             | Location, key, field path |

## Relationships

- A **schema** runs its **stages** through the **validation pipeline**, in its own **stage order**.
- **Object**, **collection**, and **dictionary** schemas differ only in *which* **stages** they have and in what order (object runs **rules** last; collection and dictionary run them first).
- A **rule engine** owns a **rule chain**; **context promotion** wraps each contextless **rule** so the same **rule** backs both schema flavours.
- A **schema** produces a **result**; a failing **result** carries one or more **validation errors**, each with a **path** and an **attempted value**.
- **Context promotion** carries a schema's **rules**, **conditionals**, and **type assertions** into its context-aware form.

## Example dialogue

> **Dev:** "The object schema runs its **rules** last but the collection runs them first — is that a bug in the **stage order**?"

> **Domain expert:** "No. Nothing short-circuits — every **stage** runs and all **validation errors** aggregate — so the order only affects the sequence of errors in the **result**. We keep each schema's **stage order** as its own declared data."

> **Dev:** "So the **validation pipeline** doesn't fix the order itself?"

> **Domain expert:** "Right. The **pipeline** just runs whatever **stages** a schema hands it, in that schema's order, and aggregates. The null-guard and **result** wrapping stay in the schema because they're the only type-specific bits."

> **Dev:** "And if I add a **refinement** with `.Refine`, which **stage** is that?"

> **Domain expert:** "It's a **rule**, so it runs in the **rules** stage via the **rule engine**. When you call `.Using<TContext>()`, **context promotion** wraps that same **rule** — you don't write a context-aware copy."

## Flagged ambiguities

- **"Context"** was overloaded three ways: the **validation run** (execution record with path/cancellation/services), the **context data** (`TContext`, the external data a context-aware schema validates against), and the **context factory** (delegate that builds that data). Keep them distinct — "context" alone should mean the validation run.
- **"Rule" vs "stage"** — a **rule** is one unit of logic (`.Min(1)`); the **rules stage** is the single **stage** in which the **rule engine** runs *all* of a schema's rules. Don't call an individual stage a "rule".
- **"Order"** was used for both **stage order** (sequence of stages) and the sample-domain `PurchaseDto` (a purchase) in the benchmarks. In this codebase "order" means **stage order** unless a purchase is clearly meant.
- **"Engine"** — reserve **rule engine** for the per-schema rule executor; do not call the **validation pipeline** an "engine".
- **"Materialize"** — specifically the lazy conversion of the **rule chain**'s linked list into an insertion-order array; not a general term for building schemas.
