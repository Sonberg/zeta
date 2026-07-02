# Contributing to Zeta

First off — thank you for taking the time to contribute ❤️  
All contributions are welcome: bug reports, ideas, docs, tests, and code.

## Ways to Contribute

You don’t need to write code to help:

- 🐞 Report bugs
- 💡 Suggest features or API improvements
- 📖 Improve documentation or examples
- 🧪 Add or improve tests
- 🧩 Implement new validators or features

If you’re unsure where to start, look for issues labeled **`good first issue`**.

---

## Getting Started

### Prerequisites
- .NET SDK 9.0+ (latest LTS recommended)
- Git
- Code editor (Visual Studio, VS Code, or Rider)

### Build & Test
```bash
# Clone the repository
git clone https://github.com/Sonberg/zeta.git
cd zeta

# Build the entire solution
dotnet build

# Run all tests
dotnet test

# Run tests with coverage (optional)
dotnet test /p:CollectCoverage=true

# Run benchmarks
dotnet run --project benchmarks/Zeta.Benchmarks -c Release

# Run the sample API
dotnet run --project samples/Zeta.Sample.Api
```

---

## Development Workflow

### 1. Create a Branch

```bash
git checkout -b feature/my-new-feature
# or
git checkout -b fix/issue-123
```

### 2. Make Your Changes

- Write code following existing patterns
- Add tests for new functionality
- Update documentation if adding features
- Ensure all tests pass: `dotnet test`

### 3. Commit Guidelines

Use clear, descriptive commit messages:

```bash
# Good commit messages
git commit -m "Add MinAge validation for DateOnly schema"
git commit -m "Fix path tracking in nested array validation"
git commit -m "Update README with RefineAsync examples"

# Less helpful
git commit -m "Update code"
git commit -m "Fix bug"
```

### 4. Submit a Pull Request

- Push your branch to GitHub
- Open a Pull Request against `main`
- Describe your changes clearly
- Link any related issues

---

## Code Guidelines

### Architecture Patterns

1. **Schema Types** - Follow existing patterns in `src/Zeta/Schemas/`
   - Create both contextless (`StringSchema`) and context-aware (`StringSchema<TContext>`) versions
   - Keep validation logic in static validators (`StringValidators`, `NumericValidators`, etc.)

2. **Validation Rules** - Implement `IValidationRule<T>` or `IAsyncValidationRule<T>`
   - Return `null` for valid input
   - Return `ValidationError` for invalid input
   - Use `execution.Path` for error paths

3. **Tests** - Add xUnit tests in `tests/Zeta.Tests/`
   - Test both success and failure cases
   - Use descriptive test names: `MethodName_Scenario_ExpectedResult`
   - Example: `Email_ValidEmail_Succeeds`

### Naming Conventions

- Use PascalCase for public members
- Use camelCase for local variables
- Use descriptive names (avoid abbreviations)
- Error codes: lowercase_with_underscores (`min_length`, `email_exists`)

### Error Messages

Keep error messages:
- Clear and actionable
- Focused on the problem (not overly technical)
- Consistent with existing messages

```csharp
// Good
"Must be at least 3 characters"
"Email already registered"
"Must be a valid email address"

// Avoid
"Validation failed"
"Invalid input"
"Error occurred"
```

---

## Adding New Schema Types

When adding a new primitive schema type:

1. Create contextless version: `MyContextlessSchema`
2. Create context-aware version: `MyContextSchema<TContext>`
3. Add static entry point to `Z` class: `Z.MyType()`
4. Ensure `.Using<TContext>()` on the contextless schema returns the context-aware variant (transferring rules, conditionals, etc.)
5. Add rule structs in `src/Zeta/Rules/<Type>/` and validation methods as extension methods (see below)
6. Add tests in `tests/Zeta.Tests/Schemas/MySchemaTests.cs`
7. Update README.md with examples
8. Add to `SchemaConsistencyTests.cs`

---

## Adding Validation Methods

Value-schema validators (string, int, double, decimal, bool, Guid, enum, DateTime, DateOnly,
TimeOnly) are **extension methods** written once and shared by both the contextless and
context-aware variants. The extension hangs off `IValueSchema<T, TSelf>` and calls the inherited
`AppendRule(...)` with a single **contextless** rule struct (implementing `IValidationRule<T>`).
The context-aware base wraps that rule automatically — do **not** add `XRule<TContext>` variants
or per-schema duplicates.

1. Add a rule struct in `src/Zeta/Rules/<Type>/` implementing `IValidationRule<T>`
2. Add one extension method to the matching `<Type>SchemaExtensions` class in `src/Zeta/Schemas/`
3. Write tests for both success and failure cases
4. Update README.md with examples
5. Update CLAUDE.md if it's a significant pattern

Example:
```csharp
// src/Zeta/Rules/String/StartsWithRule.cs
public readonly struct StartsWithRule : IValidationRule<string>
{
    private readonly string _prefix;
    private readonly string? _message;

    public StartsWithRule(string prefix, string? message = null)
    {
        _prefix = prefix;
        _message = message;
    }

    public ValueTask<ValidationError?> ValidateAsync(string value, ValidationContext context)
        => ValueTaskHelper.FromResult(
            value.StartsWith(_prefix, StringComparison.Ordinal)
                ? null
                : new ValidationError(context.PathSegments, "starts_with", _message ?? $"Must start with '{_prefix}'"));
}

// src/Zeta/Schemas/StringSchemaExtensions.cs  (namespace Zeta)
public static TSelf StartsWith<TSelf>(this IValueSchema<string, TSelf> schema, string prefix, string? message = null)
    where TSelf : IValueSchema<string, TSelf>
    => schema.AppendRule(new StartsWithRule(prefix, message));
```

`Object`/`Collection`/`Dictionary` schemas are not `IValueSchema` — their fluent methods stay as
instance/generated members.

---

## Documentation

When documenting features:

- **README.md** - User-facing examples and quick reference
- **CLAUDE.md** - Detailed architecture and patterns (for Claude Code AI)
- **docs/** - In-depth guides for specific topics
- **Code comments** - Only for complex logic (prefer clear code)

---

## Running Specific Tests

```bash
# Run tests for a specific class
dotnet test --filter "FullyQualifiedName~StringSchemaTests"

# Run a single test
dotnet test --filter "FullyQualifiedName~StringSchemaTests.Email_ValidEmail_Succeeds"

# Run tests in a specific project
dotnet test tests/Zeta.Tests

# Run tests with detailed output
dotnet test --verbosity detailed
```

---

## Performance Considerations

- Avoid allocations in hot paths
- Use `ValueTask` for async operations
- Cache compiled expressions when possible
- Profile with BenchmarkDotNet before optimizing

Run benchmarks to verify performance:
```bash
dotnet run --project benchmarks/Zeta.Benchmarks -c Release
```

---

## Questions or Issues?

- Check existing issues on GitHub
- Ask questions in discussions
- Reach out to maintainers in your PR

Thank you for contributing to Zeta!
