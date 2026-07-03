# Zeta domain & architecture vocabulary

Canonical terms for this codebase. Use these names in code, comments, and reviews.

## Validation execution

- **Schema** — a validator with an interface (`ISchema<T>` contextless, or `ISchema<T, TContext>` context-aware) and an implementation. Created via the `Z` entry point. Immutable and append-only: every fluent method returns a new instance.
- **Stage** — one step of a schema's validation, producing errors for a value: fields, type assertion, conditionals, rules, collection elements, dictionary entries/entry-refinements. A stage is a `Func<value, context, ValueTask<IReadOnlyList<ValidationError>?>>` (null/empty = no errors).
- **Validation pipeline** — `ValidationPipeline.RunAsync`, the single home of the "run every stage in order, aggregate all errors, never short-circuit" invariant. One generic runner serves both hierarchies because `ValidationContext<TContext>` derives from `ValidationContext`. Each object/collection/dictionary schema declares its own **stage order** as a memoized `Stages()` array — the order is data, not an implicit call sequence (object runs rules last; collection and dictionary run them first). The null-guard and `Result` wrapping stay in each schema's `ValidateAsync` because they are the only genuinely type-specific parts.
- **Rule chain** — `RuleChain<TRule>`, the immutable append-only list (persistent linked list with lazy insertion-order materialization) backing both rule engines.
