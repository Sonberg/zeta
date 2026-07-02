# Change log

## Next release

## 0.1.17

### Added
- `Positive()`, `Negative()`, and `MultipleOf()` on `Int` schemas, and `MultipleOf()` on `Double` schemas — closes a numeric-validator parity gap versus `Decimal` (which already had all of these).
- A synchronous `Using<TContext>(Func<T, IServiceProvider, TContext> factory)` overload on every scalar, `Collection`, and `Dictionary` contextless schema — previously only `ObjectContextlessSchema` had this convenience; other schemas required wrapping a sync factory in a `ValueTask`.
- `Result.Failure(params ValidationError[] errors)` — brings the non-generic `Result` in line with `Result<T>`/`Result<T, TContext>`, which already had this overload alongside the `IReadOnlyList<ValidationError>` one.
- `Result<T, TContext>` overloads of `ToActionResult`/`ToResult` (both the callback-taking and parameterless forms) on `ZetaExtensions` — context-aware validation results now have the same MVC/Minimal API helper parity that contextless `Result<T>` already had.
- Enum `.Each()` (Collection) and `.EachKey()`/`.EachValue()` (Dictionary) generated overloads — enum was previously the only scalar type without generated collection/dictionary support (had to fall back to `.Each(Z.Enum<T>().Defined())` manually).
- A contextless generic object `.Each(Func<ObjectContextlessSchema<TElement>, ObjectContextlessSchema<TElement>>)` overload for `Collection` schemas — the context-aware equivalent already existed, but the contextless one was missing, silently breaking the object-builder `.Each(i => i.Property(...))` pattern shown in the README's collection-validation example.
- `IValueSchema<T, TSelf>` interface (namespace `Zeta.Core`) — the extension point for value-schema validators; implemented by every value schema, contextless and context-aware.
- `IEnumerable<ValidationError>.ToPathDictionary()` extension (namespace `Zeta`) — the single home for grouping validation errors by JSONPath into the `Dictionary<string, string[]>` shape used by ASP.NET Core's `ValidationProblem` / `ValidationProblemDetails`.
- `UseZetaValidation()` extension on `EndpointDefinition` for convention-based schema auto-discovery — scans static `ISchema<TRequest>` fields on endpoint classes and registers Zeta validation as a pre-processor without any per-endpoint boilerplate.
- `ZetaGlobalPreProcessor<TRequest>` (internal, not public API) implementing `IGlobalPreProcessor` — used internally by `UseZetaValidation()`.
- `HasStarted` guard on `ZetaPreProcessor<TRequest>` and `ZetaGlobalPreProcessor<TRequest>`: skips validation if the response has already been written (safe when using both the global configurator and per-endpoint `Validate()` on the same endpoint).
- README: global registration guide (Option 3), `Refine()` cross-field validation examples, `Zeta.AspNetCore` migration guide.

### Changed
- **Breaking:** renamed the object-schema API to align with C# naming (RFC 002). `Z.Object<T>()` → `Z.Schema<T>()` and `.Field(...)` → `.Property(...)`. The old names have been removed (they previously existed as aliases). Update call sites accordingly.
- `SchemaExtensions.ValidateAsync<T>(this ISchema<T>, T? value)` no longer requires `where T : class` — it now also works for value-typed schemas (e.g. `Z.Int()`) accessed through the `ISchema<T>` interface, matching the unconstrained instance method already on the concrete schema classes.
- Renamed `ValidationFilter<T, TContext>` to `ContextValidationFilter<T, TContext>` (`Zeta.AspNetCore`) so the context-aware/contextless endpoint filter pair follows the same `Context`/`Contextless` naming convention used everywhere else in the codebase (`ContextlessValidationFilter<T>` was already correctly named).
- `Zeta.FastEndpoints.ValidationErrorExtensions.ToValidationFailures()` is now public (was `internal`), matching the core package's public `ToPathDictionary()` so FastEndpoints consumers can reuse the same error-shaping helper in custom pre-processors.
- Object schema validation order is now **Fields → Type Assertions (`.As()`) → Conditionals (`.If()`) → Rules (`.Refine()`)**, matching documented behavior (previously rules ran first). This only affects the order errors appear within `Result.Errors` when multiple validation stages fail simultaneously — no errors are added or dropped.
- `Bool`, `Guid`, `DateTime`, `DateOnly`, and `TimeOnly` validators now use dedicated rule structs (e.g. `DateTimeMinRule`, `GuidVersionRule`) instead of inline `RefinementRule<T>` closures, matching the zero-allocation pattern already used by `Int`/`Double`/`Decimal`/`String`. No behavior change — same error codes and default messages.
- Nullable object-field handling no longer uses reflection. `Property` overloads that take a pre-built schema (`ISchema<TProperty>`, `ISchema<TProperty, TContext>`, `IContextSchema<TProperty, TContext>`) for a nullable/nested property are now generic `where TProperty : class` methods that build their field validator directly — reference types need no null adapter (the inner schema already handles null). Nullable **value-type** fields continue to be served by the source generator's typed overloads (`int?`, `decimal?`, …) and the fluent builders. Call sites are unchanged.
- The dedicated field-error path-remapping logic (`RelativeTo`/`Concat`) is now shared via `FieldError.PrependFieldPath` instead of being copy-pasted across the four field validators.
- Value-schema validators (`string`, `int`, `double`, `decimal`, `bool`, `Guid`, enum, `DateTime`, `DateOnly`, `TimeOnly`) are now **extension methods** on `IValueSchema<T, TSelf>` (in `*SchemaExtensions` classes, namespace `Zeta`) instead of instance methods duplicated across the contextless and context-aware schema classes. Call sites are unchanged (`Z.String().Email()`, `Z.String().Using<Ctx>().Email()`); each validator is now defined once. `Object`/`Collection`/`Dictionary` schemas are unaffected.
- Context factory resolution ("resolve factory → promote to `ValidationContext<TContext>` → validate") is now centralized in `ContextFactoryResolver.ResolveAndValidateAsync`, shared by the contextless `ISchema<T>` self-resolving bridge and `ZetaValidator`. No public API change.
- ASP.NET Core filters/result helpers and FastEndpoints pre-processors now route error shaping through single extensions (`ToPathDictionary()` / `ToValidationFailures()`) instead of inline `GroupBy`/`ValidationFailure` loops.
- NuGet package description updated to clarify pre-processor (not middleware) integration model.
- `PackageReadmeFile` now correctly packs `README.md` (FastEndpoints-specific) instead of the root `README.md`.
- Context-aware collection and dictionary count rules (`MinLength`/`MaxLength`/`Length`/`NotEmpty`) now flow through the existing `ContextlessRuleAdapter<T, TContext>` via `AppendRule`, matching the value-schema rules. No public API change.

### Fixed
- `DateOnlyContextlessSchema`'s constructor is now `internal`, matching every other scalar schema. It was previously `public`, the sole exception among 20 scalar-schema classes, allowing construction that bypassed `Z.DateOnly()`.
- CHANGELOG: clarified that `ZetaGlobalPreProcessor<TRequest>` is internal, not public API, and noted that the `AddZeta(Assembly[])` overload marked obsolete in 0.1.12 has since been fully removed (only `AddZeta()` remains).
- `ValidationContext.Path` (and `ValidationContext<TData>.Path`) at the root now renders as `"$"` instead of `""`, matching the `"$"` root convention already used by `ValidationError.Path`/`PathString`. Non-root paths are unaffected (still bare dot/bracket notation, no `"$."` prefix).

### Removed
- The nullable-adapter matrix (`NullableAdapterFactory` and the six `Nullable{Reference,Struct}Context{,less}{Adapter,Wrapper}` classes) and the reflection (`Activator.CreateInstance`) it relied on. This makes nullable field construction NativeAOT/trim-safe. Passing a **pre-built** `ISchema<TStruct>` for a nullable value-type field is no longer supported (it previously threw `ArgumentException` at runtime); use the fluent builder or a source-generated overload instead.
- Redundant context-aware rule structs (`MinLengthRule<TContext>`, `MaxIntRule<TContext>`, `DefinedRule<TEnum, TContext>`, and the rest — 28 in total). Context-aware validation of these rules now flows through `ContextlessRuleAdapter<T, TContext>` — the same path `.Using<TContext>()` already used.
- The remaining context-aware collection/dictionary count-rule twins (`MinLengthRule<T, TContext>`, `MaxLengthRule<T, TContext>`, `LengthRule<T, TContext>`, `NotEmptyRule<T, TContext>`, and the three `Dictionary*Rule<TKey, TValue, TContext>` variants). These were context-blind duplicates now served by `ContextlessRuleAdapter<T, TContext>`. (The context-*consuming* `RefinementRule<T, TContext>`, `StatefulRefinementRule<T, TContext, TState>`, and `EntryRefinement<…, TContext>` are unchanged.)
- `RequiredFieldContextlessValidator` and `RequiredFieldContextContextValidator` — dead code with no production construction sites (nullable/required field validation is handled by the source-generated field overloads and `NullableField*Validator`).

### Tests
- Closed remaining public-API test gaps found by an audit against the "every public method must be tested" rule: the non-generic `Result.Success()`/`Result.Failure()`, `ValidationErrorExtensions.ToPathDictionary()`, `Z.Context()`, the `RefineAt` message-factory and value-only-predicate overloads on `ObjectContextlessSchema`/`ObjectContextSchema`, the `ZetaExtensions.ToActionResult`/`ToResult` callback overloads (including the `Result<T, TContext>` variant), `DependencyInjectionExtensions.AddZeta()`, and all three `ZetaExtensions.WithValidation` overloads (now covered end-to-end via an in-memory `TestServer`-backed minimal API).

## 0.1.16

### Added
- `Zeta.FastEndpoints` NuGet package: `ZetaPreProcessor<TRequest>` for FastEndpoints pre-processor pipeline integration. Handles both contextless and context-aware schemas — context-aware schemas with `.Using<TContext>(factory)` self-resolve context from `IServiceProvider`, requiring no separate pre-processor type. Includes `Zeta.Sample.FastEndpoints.Api` sample with inline schema definitions per endpoint.

## 0.1.15

### Added

- **`RefineEachEntry`/`RefineEachEntryAsync` for dictionary schemas**: Per-entry predicate validation on `DictionaryContextlessSchema` and `DictionaryContextSchema`. Each failing entry produces one `ValidationError` at `$[keyString]` (bracket notation). Supports value-only, value+context, and async-with-CT predicate overloads. Entry refinements transfer automatically when calling `.Using<TContext>()`.
- **`ValidationContext.PushKey(string)`**: New path method for bracket-notation dictionary key paths (e.g. `$.schedule[2024W15]`).
- **Structured path segments**: `ValidationContext` now stores an immutable linked list of typed path segments (`Property`, `Index`, `DictionaryKey`) internally. Rendering to string is lazy and cached per node. Eliminates fragile raw string concatenation. `PushKey<TKey>(TKey key)` replaces the old `PushKey(string)` — the key is stored as-is; `ToString()` is only called at render time (when a `ValidationError` is created). No behavior change at the API surface.

### Removed

- Removed `netstandard2.0` target from `Zeta`; supported targets are now .NET 6/7/8/9/10.

## 0.1.14

### Added

- **Dictionary schemas**: `Z.Dictionary<TKey, TValue>()` and `Z.Dictionary(keySchema, valueSchema)` for validating `IDictionary<TKey, TValue>`. Supports `.EachKey()`, `.EachValue()`, `.MinLength()`, `.MaxLength()`, `.NotEmpty()`, `.Nullable()`, `.If()`, and `.Refine()`. Context-aware promotion via `.Using<TContext>()`. Object field support via generated `Field()` overloads for both `IDictionary<TKey, TValue>` and `Dictionary<TKey, TValue>` properties. Key errors are reported at `$.keys[N]`, value errors at `$.<key>`.

- **`Result<T, TContext>` type**: Context-aware validation now returns `Result<T, TContext>` (extends `Result<T>`). Provides both `.Value` (the validated input) and `.Context` (the resolved context data). All monadic operations (`.Map()`, `.Then()`, `.Match()`, `.GetOrDefault()`, `.GetOrThrow()`) continue to work via inheritance.

- Added a new Blazor sample app at `samples/Zeta.Sample.Blazor` demonstrating interactive form validation with Zeta schemas.

### Breaking

- **`ISchema<T, TContext>.ValidateAsync` return type changed**: Now returns `ValueTask<Result<T, TContext>>` instead of `ValueTask<Result>`. Callers that assigned the result to `Result` can now use `Result<T, TContext>` or `Result<T>` (both are valid since `Result<T, TContext>` extends `Result<T>`).

- **`IZetaValidator.ValidateAsync<T, TContext>` return type changed**: Now returns `ValueTask<Result<T, TContext>>` instead of `ValueTask<Result<T>>`. Existing code assigning to `Result<T>` continues to work without changes.

- **`SchemaExtensions.ValidateAsync<T, TContext>` return type changed**: The convenience extension method now returns `ValueTask<Result<T, TContext>>` instead of `ValueTask<Result<T>>`.

- **`ISchema<in T, TContext>` variance removed**: `T` is now invariant (was contravariant `in T`). Required to allow `T` to appear in the return type `Result<T, TContext>`. Assignments relying on contravariant `T` will need explicit adapters.


## 0.1.13

### Added

- **`RefineAt(...)` for object schemas**: Attach object-level refinement errors to a specific property path instead of root (`$`). Supports contextless and context-aware object schemas, including dynamic message factories.
- **Enum schemas**: Added `Z.Enum<TEnum>()` with `.Defined()` and `.OneOf(...)` (contextless and context-aware), plus object `.Field/.Property(...)` support for enum and nullable enum properties.

### Fixed

- **Context-aware schemas now implement `ISchema<T>`**: Schemas created via `.Using<TContext>(factory)` can now be assigned to `ISchema<T>` variables directly. Validation uses the embedded factory to self-resolve context via `IServiceProvider`.
- **`ObjectContextSchema` supports pre-built contextless field schemas**: `Field(p => p.Name, Z.String()...)` now works on context-aware object schemas, mirroring the contextless `ObjectContextlessSchema` API.
- **`ObjectContextSchema.AddField`/`AddContextlessField` now preserve conditionals**: Previously, calling `.Field()` after `.If()` silently dropped conditionals.
- **`ZetaValidator.ValidateAsync<T, TContext>` now propagates `IServiceProvider`** to the typed `ValidationContext<TContext>`.

### Breaking

- **Immutable, append-only schemas**: Every fluent method (`.MinLength()`, `.Field()`, `.Nullable()`, `.If()`, etc.) now returns a **new schema instance** instead of mutating `this`. Schema reuse and branching are now safe — modifying a branched schema never affects the original. Rule engines use persistent linked lists with lazy materialization for O(1) append and structural sharing.

- **`Action<TSchema>` overloads of `.If()` removed**: Use `Func<TSchema, TSchema>` overloads instead. With immutability, Action callbacks cannot capture the mutated state.

- **`.As<TDerived>()` no longer mutates parent**: The return value must be captured and composed via `.If()`. The recommended pattern is `.If(x => x is Dog, dogSchema)`.

- **`.SetContextFactory()` replaced with `.WithContextFactory()`**: Returns a new schema instance instead of mutating.

### Fixed

- **Collection `.Each()` extension methods now preserve `AllowNull` and conditionals**: Previously, generated `.Each()` extensions lost `AllowNull` and conditional state when creating new collection schemas.

- **Context-aware `CollectionContextSchema.Each()` now preserves conditionals**: Previously passed `null` for conditionals, losing collection-level conditional validation.

- Prefix error paths with $ to clearly distinguish them from property names and avoid confusion with nested properties. For example, an error on the root value would have path `"$"` instead of `""`, and a field error would have path `"$.fieldName"` instead of `"fieldName"`. This makes it clear that paths are error paths and prevents ambiguity with property names.

## 0.1.12


### Breaking

- **Renamed `.WithContext<TContext>()` to `.Using<TContext>()`** on all contextless schema types. The new name better communicates the intent of promoting a schema to context-aware validation.

- **Removed `IValidationContextFactory<TInput, TContext>`** — context factories are now inline delegates passed directly to `.Using<TContext>(factory)` instead of separate classes registered via DI assembly scanning. This simplifies the API and eliminates the need for factory classes and assembly scanning.

- **`AddZeta(Assembly[])` is now obsolete** — use `AddZeta()` without parameters. Assembly scanning for context factories is no longer needed.

### Added

- **Self-resolving context schemas in `.If()` branches**: Context-aware schemas with factories (`.Using<TContext>(factory)`) can now be passed directly to `.If()` on contextless object schemas. The root schema stays contextless while each branch independently resolves its own context via `IServiceProvider` from `ValidationContext`. This enables polymorphic validation where different branches use different contexts without promoting the entire schema tree.

- **`IServiceProvider` on `ValidationContext`**: Added optional `IServiceProvider?` property to `ValidationContext` and `ValidationContext<TData>`, propagated through `Push()` and `PushIndex()`. `ValidationContextBuilder.Build()` now passes the configured service provider to the built context.

### Breaking

- **Removed `WhenType<TTarget>()` and context-promoting `If` overloads** from `ObjectContextlessSchema` and `ObjectContextSchema`. These overloads promoted the entire root schema to context-aware, requiring all branches to share the same `TContext`. Use self-resolving schemas instead: build each branch as a separate schema with `.Using<TContext>(factory)` and pass it to `.If(predicate, schema)`.

### Fixed

- **`ValidationContextBuilder.Build()` now passes `IServiceProvider`**: Previously, the builder stored the service provider but did not pass it through to the built `ValidationContext`.

## 0.1.11
### Added

- **`.Using<TContext>(factory)` with inline factory delegate**: Context-aware schemas now accept an optional `Func<T, IServiceProvider, CancellationToken, Task<TContext>>` factory delegate that is stored on the schema and used automatically by the ASP.NET Core integration to create context data before validation.

- **`IContextFactorySchema<T, TContext>` interface**: New interface on `ContextSchema` base class providing access to the schema's built-in factory delegate for cross-assembly usage.

- **`.As<T>()` type assertion for polymorphic validation**: Added `.As<TDerived>()` to `ObjectSchema` (both contextless and context-aware) for runtime type narrowing during validation. When the value matches `TDerived`, validation continues with a type-narrowed schema; otherwise emits a `type_mismatch` error. Composes naturally with `.If()` for safe polymorphic validation. Type assertions are preserved through `.WithContext()` promotion (RFC 004).

- **`.If<TDerived>()` overload for concise polymorphic validation**: Added `.If<TDerived>(configure)` to `ObjectSchema` (both contextless and context-aware). Combines type checking with `.As<TDerived>()` in a single call: `Z.Object<IAnimal>().If<Dog>(dog => dog.Field(x => x.WoofVolume, x => x.Min(0).Max(100)))`.

- **`.If()` guard-style conditional validation**: Added `.If(predicate, configure)` to all schema types (value, object, collection) for guard-style conditional validation without else branches. Supports chaining multiple `.If()` guards, nesting, and context promotion via `.Using()`. Context-aware schemas support both value-only `If(v => ..., ...)` and value+context `If((v, ctx) => ..., ...)` predicates.

- **Support for additional runtimes**
  - Zeta now supports .NET 6, 7, 8, and 10 (in addition to .NET Standard 2.0), allowing users to take advantage of the latest C# features and performance improvements while maintaining broad compatibility.
  - Zeta.AspNetCore integration package now targets .NET 8, 9 and 10, enabling seamless validation in ASP.NET Core applications across all supported runtimes.

### Removed

- Deleted old `Conditional/` infrastructure (unused `ConditionalBuilder`, `ConditionalBranch`, etc.)

## 0.1.10

### Tests

- **Expanded schema-level test coverage**: Added 26 new tests covering gaps in `DoubleSchema.Negative()`, `DecimalSchema.Max()`/`Negative()`, `StringSchema.MaxLength()`/`Regex()`/`NotEmpty()`/`RefineAsync()`/custom messages, `CollectionSchema.Length(exact)`, `GuidSchema.Version()` failure case, and source-generated `Field()` builders for `bool`, `Guid`, `DateTime`, `DateOnly`, and `TimeOnly` types.

### Fixed

- **InvalidCastException with non-nullable value type pre-built schemas**: Fixed `InvalidCastException` when passing a pre-built schema (e.g., `Z.Int().Min(0)`) to `.Field()` for non-nullable value type properties (`int`, `double`, `decimal`, `bool`, `Guid`, `DateTime`, `DateOnly`, `TimeOnly`). Previously, these fell through to the generic `Field<TProperty>` method which incorrectly attempted nullable wrapping. Added generated overloads for `ISchema<T>` on non-nullable value type properties.

- **Nullable value type field error path**: Fixed `NullableFieldContextlessValidator` using the parent path instead of the field-pushed path for `null_value` errors, causing incorrect error paths (e.g., `""` instead of `"nullableInt"`).

- **AllowNull not transferred by WithContext()**: `.Nullable()` called before `.WithContext<TContext>()` now correctly carries over to the context-aware schema. Previously, `AllowNull` was lost during context promotion, causing null values to be rejected even when `.Nullable()` was set. Fixed for all schema types (String, Int, Double, Decimal, Bool, Guid, DateTime, DateOnly, TimeOnly, Collection, Object).

### Added

- **Nullable value type fields**: Nullable value type properties (`int?`, `double?`, `decimal?`, `bool?`, `Guid?`, `DateTime?`, `DateOnly?`, `TimeOnly?`) work directly in `.Field()` methods. Null values skip validation automatically — no need to call `.Nullable()`.

  ```csharp
  public record User(int Id, int? Age, decimal? Balance, string? Bio);

  var schema = Z.Object<User>()
      .Field(x => x.Id, s => s.Min(1))
      .Field(x => x.Age, s => s.Min(0).Max(120))        // null skips validation
      .Field(x => x.Balance, Z.Decimal().Positive())     // null skips validation
      .Field(x => x.Bio, s => s.MaxLength(500));         // string? needs .Nullable() on schema if null should pass
  ```

  Works with both inline builders and pre-built schemas, for contextless and context-aware schemas.

- **Null checks in ObjectSchema and CollectionSchema**: `ValidateAsync` overrides now check for null at the top, returning a `null_value` error unless `.Nullable()` is called on the schema.

- **Nullable reference type field support**: `string?` properties no longer produce nullable warnings when used with inline field builders.

- **Nullable value type field validators**: Added `NullableFieldContextlessValidator` and `NullableFieldContextContextValidator` with `where TProperty : struct` constraint, properly handling `Nullable<TProperty>` getters. Null values skip validation; non-null values are unwrapped and validated. Fixes CS0266 in all generated nullable value type field overloads.

## 0.1.9
- Major invalid-path optimizations
- Reduced allocations on successful validation (-39%)
- Eliminated lambda and closure overhead in built-in rules
- Zero-allocation refinement infrastructure

## Version 0.1.8
- Add `.Each()` method for collection schemas to enable fluent element validation (RFC 003)
- Add `.Each()` support for inline object builders - validate complex object collections without pre-building schemas
- **Breaking:** Change `Z.Collection()` to be parameterless with generic type parameter (e.g., `Z.Collection<string>()` instead of `Z.Collection(Z.String())`)
- Refactor `CollectionSchemaExtensions` to be source-generated for maintainability

## Version 0.1.7
- Add fluent field support for ObjectSchema with source-generated properties
- Support for nested Collection and Object schemas
- Merge Array and List schema into unified CollectionSchema
- Update to .NET 10
- Remove StackTrace from validation errors
- Improve benchmarks and documentation

## Version 0.1.6
- Add ValidationContextBuilder for easier context creation
- Remove IServiceProvider dependency from ContextFactory
- Add implicit operator to builder
- Merge CancellationToken & ValidationContext
- Split and rename files for better organization
- Convert contextless schemas to context-aware using WithContext<TContext>()
- Move WithContext as instance method on schemas
- Adjust namespaces and folder structure

## Version 0.1.5
- Add .WithContext() for schema context promotion
- Add Select method to conditional validation
- Improve schema inference
- Make schemas contextless by default
- Handle no-context scenarios

## Version 0.1.4
- Add BaseSchema optimization
- Singleton pattern for Result.Success
- Add TimeProvider support for testability
- Target .NET Standard 2.0
- Improve test coverage

## Version 0.1.3
- Improve sample application
- Add CLAUDE.md documentation
- Update README with more examples
- Add more built-in schema types
- Change from Task to ValueTask for better performance

## Version 0.1.2
- Fix nullable schema validation
- Add tests for nullable scenarios
- Improve interface consistency

## Version 0.1.1
- Fix no-context validation scenarios
- Improve schema extensions

## Version 0.1
- Initial release
- Core validation framework with Result pattern
- Support for string, int, double, decimal, bool, Guid, DateTime, DateOnly, TimeOnly schemas
- ObjectSchema with field validation
- ArraySchema and ListSchema for collections
- Async validation support
- ASP.NET Core integration
- Benchmarks vs FluentValidation and DataAnnotations
