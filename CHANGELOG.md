# Change log
- Remove .When - Will be reimplemented

## Next release
- **Performance:** Eliminate closure allocations in built-in validation methods using stateful refinement rules
  - Introduced `StatefulRefinementRule<T, TState>` and `StatefulRefinementRule<T, TContext, TState>` that use static lambdas with value-type state
  - Updated String, Int, Double, Decimal, and Collection schemas (both contextless and context-aware)
  - **Result:** Zero allocations during validation for built-in validators (benchmarks confirm 0 B allocated)
  - User-provided `Refine()` predicates may still allocate (acceptable for backward compatibility)
- Fix array field overloads to correctly return `CollectionContextlessSchema<T>` instead of `ObjectContextlessSchema<T[]>` for inline array field builders
- Refactor source generators into separate files for better maintainability:
  - `SchemaMapping.cs` - Shared type-to-schema mappings
  - `ObjectSchemaFieldGenerator.cs` - ObjectContextlessSchema field overloads
  - `ObjectContextSchemaFieldGenerator.cs` - ObjectContextSchema field overloads
  - `CollectionExtensionsGenerator.cs` - Collection .Each() extension methods
  - `SchemaFactoryGenerator.cs` - Main orchestrator

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