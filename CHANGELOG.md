# Change log
- Remove .When - Will be reimplemented

## Next release

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

## 1.0.9
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