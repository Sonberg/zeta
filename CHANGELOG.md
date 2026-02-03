# Change log
- Remove .When - Will be reimplemented

## Next release
- **Performance:** Optimize field validators and Result.Combine() to reduce allocations in invalid cases
  - Removed unnecessary `.ToList()` conversion in `FieldContextlessValidator` and `FieldContextContextValidator` - errors are already `IReadOnlyList<ValidationError>`
  - Optimized `Result.Combine()` to use single-pass enumeration instead of multiple LINQ operations (3 allocations reduced to 1)
  - Added path caching in `ValidationContext.Push()` using `ConcurrentDictionary` to reduce string allocations for frequently validated nested paths
  - Created `ValueTaskHelper.FromResult()` to use optimal ValueTask creation (`ValueTask.FromResult()` on .NET Standard 2.1+, `new ValueTask<T>()` on .NET Standard 2.0)
  - **Result:** Invalid case improved from 446.5 ns / 1,096 B to 398.6 ns / 904 B (10.7% faster, 17.5% less memory)
  - Valid case improved from 296.9 ns to 293.2 ns (1.2% faster), allocations unchanged at 152 B
  - Still 4.8x faster and 8.5x less memory than FluentValidation on invalid input
- **Testing:** Add comprehensive unit tests for all validation rules
  - Added `StringRuleTests` with 57 tests covering all string validation rules
  - Added `NumericRuleTests` with 54 tests covering int, double, and decimal rules
  - Added `CollectionRuleTests` with 28 tests covering collection validation rules
  - Tests verify correct behavior for valid inputs, invalid inputs, edge cases, and custom error messages
  - Total test count increased from 379 to 518 tests (139 new tests)
- **Refactoring:** Move all validation logic from static validator classes into rule structs
  - Moved validation logic from `StringValidators`, `NumericValidators`, and `CollectionValidators` directly into rule implementations
  - Each rule is now fully self-contained with its own validation logic
  - Removed `Validation/` folder and all static validator classes
  - **Benefits:** Simpler architecture, fewer indirections, easier to understand and maintain
  - No performance impact - all tests pass with identical behavior
- **Performance:** Optimize validation infrastructure to reduce allocations by 39%
  - Cached empty error lists in `FieldContextlessValidator` and `FieldContextContextValidator` to avoid `.ToList()` allocations on successful validations
  - Optimized string concatenation in `ValidationContext.Push()` to use `string.Concat()` instead of interpolation
  - **Result:** Allocations reduced from 248 B to 152 B (96 B / 39% reduction) for successful validations
  - Validation speed unchanged, GC pressure significantly reduced
- **Performance:** Replace `StatefulRefinementRule` with dedicated validation rule structs to eliminate all lambda overhead
  - Created dedicated readonly struct rules in `Rules/String/`, `Rules/Numeric/`, and `Rules/Collection/` folders
  - **String rules:** `MinLengthRule`, `MaxLengthRule`, `LengthRule`, `NotEmptyRule`, `EmailRule`, `UuidRule`, `UrlRule`, `UriRule`, `AlphanumericRule`, `StartsWithRule`, `EndsWithRule`, `ContainsRule`, `RegexRule`
  - **Numeric rules:** `MinIntRule`, `MaxIntRule`, `MinDoubleRule`, `MaxDoubleRule`, `MinDecimalRule`, `MaxDecimalRule`, `PositiveDoubleRule`, `PositiveDecimalRule`, `NegativeDoubleRule`, `NegativeDecimalRule`, `FiniteRule`, `PrecisionRule`, `MultipleOfRule`
  - **Collection rules:** `MinLengthRule<T>`, `MaxLengthRule<T>`, `LengthRule<T>`, `NotEmptyRule<T>`
  - Each rule implements `IValidationRule<T>` directly without delegates or tuples
  - Updated all String, Int, Double, Decimal, and Collection schemas (both contextless and context-aware) to use dedicated rules
  - **Benefits:** Zero lambda overhead, clearer types (`EmailRule` vs `StatefulRefinementRule<string, string?>`), better IDE support
  - **Result:** Same zero-allocation performance with cleaner, more maintainable code
  - `StatefulRefinementRule` remains available for user-defined custom rules
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
- Remove .When (Will be reimplemented as .If & .Switch in RFC 001)
- Make .Each more stable by removing inline object builders (if caused stability issues)
- .Each only support ISchema<T> or ISchema<T, TContext> (no inline builders)
- Rename .Z.Object() to .Z.Schema() (RFC 002). To algin with C# language
- Rename .Field() to .Property() (RFC 002). To algin with C# language

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