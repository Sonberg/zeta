# Change log
- Remove .When - Will be reimplemented

## Next release

### Breaking

- **Immutable, append-only schemas**: Every fluent method (`.MinLength()`, `.Field()`, `.Nullable()`, `.If()`, etc.) now returns a **new schema instance** instead of mutating `this`. Schema reuse and branching are now safe — modifying a branched schema never affects the original. Rule engines use persistent linked lists with lazy materialization for O(1) append and structural sharing.

- **`Action<TSchema>` overloads of `.If()` removed**: Use `Func<TSchema, TSchema>` overloads instead. With immutability, Action callbacks cannot capture the mutated state.

- **`.As<TDerived>()` no longer mutates parent**: The return value must be captured and composed via `.If()`. The recommended pattern is `.If(x => x is Dog, dogSchema)`.

- **`.SetContextFactory()` replaced with `.WithContextFactory()`**: Returns a new schema instance instead of mutating.

### Fixed

- **Collection `.Each()` extension methods now preserve `AllowNull` and conditionals**: Previously, generated `.Each()` extensions lost `AllowNull` and conditional state when creating new collection schemas.

- **Context-aware `CollectionContextSchema.Each()` now preserves conditionals**: Previously passed `null` for conditionals, losing collection-level conditional validation.

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
