# Change log
- Remove .When - Will be reimplemented

## Next release (v2.0.0 - MAJOR BREAKING CHANGES)

### ⚠️ BREAKING CHANGES: Nullable Value Type Handling

**This release fundamentally changes how nullable value types work in Zeta. All numeric schemas now use nullable type parameters (int?, double?, etc.) with an IsNullable flag to control null validation.**

#### Type System Changes
- **ALL value type schemas now use nullable type parameters:**
  - `IntContextlessSchema` changed from `ContextlessSchema<int>` to `ContextlessSchema<int?>`
  - `DoubleContextlessSchema` changed from `ContextlessSchema<double>` to `ContextlessSchema<double?>`
  - `DecimalContextlessSchema` changed from `ContextlessSchema<decimal>` to `ContextlessSchema<decimal?>`
  - `BoolContextlessSchema` changed from `ContextlessSchema<bool>` to `ContextlessSchema<bool?>`
  - `GuidContextlessSchema` changed from `ContextlessSchema<Guid>` to `ContextlessSchema<Guid?>`
  - `DateTimeContextlessSchema` changed from `ContextlessSchema<DateTime>` to `ContextlessSchema<DateTime?>`
  - `DateOnlyContextlessSchema` changed from `ContextlessSchema<DateOnly>` to `ContextlessSchema<DateOnly?>`
  - `TimeOnlyContextlessSchema` changed from `ContextlessSchema<TimeOnly>` to `ContextlessSchema<TimeOnly?>`
  - Same changes apply to all context-aware variants

#### Return Type Changes
- `Z.Int()` now returns `ISchema<int?>` instead of `ISchema<int>`
- `Z.Double()` now returns `ISchema<double?>` instead of `ISchema<double>`
- All numeric factory methods follow the same pattern
- **User code with explicit type declarations will break** and must be updated

#### API Changes
- `.Nullable()` and `.Optional()` extensions now **set the IsNullable flag** instead of creating wrapper classes
- Wrapper classes (`NullableValueContextlessSchema<T>`, `NullableValueContextSchema<T, TContext>`, etc.) are **marked [Obsolete]**
- Schemas are non-nullable by default (IsNullable = false)
- Calling `.Nullable()` sets IsNullable = true and returns the same schema instance

#### Null Validation Behavior
- By default, null values return a "required" validation error
- After calling `.Nullable()`, null values pass validation without running inner rules
- Base classes (`ContextlessSchema<T>`, `ContextSchema<T, TContext>`) now check IsNullable flag in ValidateAsync

#### Migration Guide

**Before (v1.x):**
```csharp
// Explicit types
ISchema<int> schema = Z.Int().Min(5);
Result<int> result = await schema.ValidateAsync(42);
int value = result.Value;  // int

// Nullable
var nullableSchema = Z.Int().Nullable();  // Returns wrapper class
```

**After (v2.0.0):**
```csharp
// Explicit types - MUST use nullable
ISchema<int?> schema = Z.Int().Min(5);
Result<int?> result = await schema.ValidateAsync(42);
int? value = result.Value;  // int?

// If you know it's valid, unwrap:
if (result.IsSuccess && result.Value.HasValue)
{
    int actualValue = result.Value.Value;
}

// Nullable - same API, different implementation
var nullableSchema = Z.Int().Nullable();  // Sets flag, returns same instance
```

**Type inference still works (recommended):**
```csharp
// No explicit types - code continues to work
var schema = Z.Int().Min(5);
var result = await schema.ValidateAsync(42);
```

### Internal Changes
- Added `IsNullable` protected field to `ContextlessSchema<T>` and `ContextSchema<T, TContext>`
- Added `MarkAsNullable()` public method to base schema classes
- All numeric validation rules now accept nullable types with defensive null checking
- Updated all schema methods (Refine, RefineAsync, IsTrue, IsFalse, etc.) to handle nullable types

### Known Issues (Work In Progress)
- Source generators not yet updated to handle nullable type parameters
- Some conditional builder methods need updates for type parameter compatibility
- Test suite requires updates for new type system
- Sample applications need migration

### Notes
This is a **major breaking change** that affects all code using explicit type declarations for numeric schemas. Code using type inference should continue to work with minimal changes. This change enables better null handling semantics and eliminates the need for wrapper classes.

---

### Other Changes in This Release
- Fix context factory exceptions propagating as unformatted HTTP 500; now returns a root-level validation error instead
- Fix null values on required fields crashing rules instead of returning a "required" validation error; schemas now check `IsNullable` flag to distinguish required vs optional

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