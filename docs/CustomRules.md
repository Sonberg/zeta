# Custom Rules

This guide covers creating custom validation rules in Zeta, from simple inline refinements to reusable rule classes.

---

## Inline Refinements

The simplest way to add custom validation is with `.Refine()`:

```csharp
Z.String()
    .Refine(
        value => value.StartsWith("A"),
        message: "Must start with A",
        code: "starts_with_a"
    );
```

The `code` parameter is optional and defaults to `"custom_error"`:

```csharp
Z.String().Refine(s => s.Contains("@"), "Must contain @");
// Error code will be "custom_error"
```

---

## Context-Aware Refinements

When using context-aware schemas, refinements can access the context:

```csharp
Z.String()
    .Using<UserContext>()
    .Refine(
        (value, ctx) => !ctx.BannedEmails.Contains(value),
        message: "Email is banned",
        code: "email_banned"
    );
```

You can mix context-free and context-aware refinements:

```csharp
Z.String()
    .Using<UserContext>()
    .Refine(s => s.Length >= 3, "Too short")              // No context
    .Refine((s, ctx) => s != ctx.Reserved, "Reserved");   // With context
```

---

## Async Context-Aware Refinements

Use `RefineAsync` for async validation with context:

```csharp
Z.String()
    .Email()
    .Using<UserContext>()
    .RefineAsync(
        async (email, ctx, ct) => !await ctx.Repo.EmailExistsAsync(email, ct),
        message: "Email already registered",
        code: "email_exists"
    );
```

You can combine sync and async refinements:

```csharp
Z.String()
    .Email()
    .Using<UserContext>()
    .Refine((email, ctx) => !ctx.BannedDomains.Any(d => email.EndsWith(d)),
        "Domain not allowed")
    .RefineAsync(async (email, ctx, ct) =>
        !await ctx.Repo.EmailExistsAsync(email, ct),
        "Email already registered");
```

**Note**: `RefineAsync` also has a contextless overload:

```csharp
Z.String()
    .Using<UserContext>()
    .RefineAsync(async (value, ct) =>
        await SomeExternalValidationAsync(value, ct),
        "External validation failed");
```

---

## Custom Rule Classes

For reusable validation logic, implement `IValidationRule<T>`:

```csharp
public sealed class StartsWithUpperRule : IValidationRule<string>
{
    public ValidationError? Validate(string value, ValidationExecutionContext execution)
    {
        return char.IsUpper(value[0])
            ? null
            : new ValidationError(execution.Path, "starts_upper", "Must start with uppercase");
    }
}

// Usage
Z.String().Use(new StartsWithUpperRule());
```

### Parameterized Rules

Rules can accept parameters:

```csharp
public sealed class ContainsDigitsRule : IValidationRule<string>
{
    private readonly int _minDigits;

    public ContainsDigitsRule(int minDigits)
    {
        _minDigits = minDigits;
    }

    public ValidationError? Validate(string value, ValidationExecutionContext execution)
    {
        var digitCount = value.Count(char.IsDigit);

        return digitCount >= _minDigits
            ? null
            : new ValidationError(
                execution.Path,
                "min_digits",
                $"Must contain at least {_minDigits} digits");
    }
}

// Usage
Z.String().Use(new ContainsDigitsRule(2));
```

---

## Context-Aware Rule Classes

For rules that need access to validation context, implement `IValidationRule<T, TContext>`:

```csharp
public interface IEmailContext
{
    bool EmailExists { get; }
}

public sealed class UniqueEmailRule<TContext> : IValidationRule<string, TContext>
    where TContext : IEmailContext
{
    public ValidationError? Validate(string value, ValidationContext<TContext> ctx)
    {
        return ctx.Data.EmailExists
            ? new ValidationError(ctx.Execution.Path, "email_taken", "Email already taken")
            : null;
    }
}

// Usage
Z.String()
    .Using<UserContext>()
    .Use(new UniqueEmailRule<UserContext>());
```

### Generic Context Constraints

Use interfaces for flexible context requirements:

```csharp
public interface IHasBannedWords
{
    IReadOnlySet<string> BannedWords { get; }
}

public sealed class NoBannedWordsRule<TContext> : IValidationRule<string, TContext>
    where TContext : IHasBannedWords
{
    public ValidationError? Validate(string value, ValidationContext<TContext> ctx)
    {
        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var banned = words.FirstOrDefault(w => ctx.Data.BannedWords.Contains(w.ToLower()));

        return banned is null
            ? null
            : new ValidationError(
                ctx.Execution.Path,
                "banned_word",
                $"Contains banned word: {banned}");
    }
}
```

---

## Async Rules

For rules that need async operations, implement `IAsyncValidationRule<T>`:

```csharp
public sealed class UniqueUsernameRule : IAsyncValidationRule<string>
{
    private readonly IUserRepository _repo;

    public UniqueUsernameRule(IUserRepository repo)
    {
        _repo = repo;
    }

    public async ValueTask<ValidationError?> ValidateAsync(
        string value,
        ValidationExecutionContext execution)
    {
        var exists = await _repo.UsernameExistsAsync(value, execution.CancellationToken);

        return exists
            ? new ValidationError(execution.Path, "username_taken", "Username already taken")
            : null;
    }
}
```

Context-aware async rules:

```csharp
public sealed class AsyncContextRule<TContext> : IAsyncValidationRule<string, TContext>
{
    public async ValueTask<ValidationError?> ValidateAsync(
        string value,
        ValidationContext<TContext> ctx)
    {
        // Use ctx.Data for context
        // Use ctx.Execution.CancellationToken for cancellation
        // Use ctx.Execution.Services for DI
        await Task.Delay(1);
        return null;
    }
}
```

---

## Accessing Services in Rules

Use `ValidationExecutionContext.Services` for dependency injection:

```csharp
public sealed class ExternalValidationRule : IAsyncValidationRule<string>
{
    public async ValueTask<ValidationError?> ValidateAsync(
        string value,
        ValidationExecutionContext execution)
    {
        var service = execution.Services.GetRequiredService<IExternalValidator>();
        var isValid = await service.ValidateAsync(value, execution.CancellationToken);

        return isValid
            ? null
            : new ValidationError(execution.Path, "external_invalid", "External validation failed");
    }
}
```

---

## Combining Multiple Rules

Rules can be combined on a single schema:

```csharp
Z.String()
    .Use(new StartsWithUpperRule())
    .Use(new ContainsDigitsRule(2))
    .Use(new MaxLengthRule(50))
    .Refine(s => !s.Contains("admin"), "Cannot contain 'admin'");
```

Rules are executed in order, and all errors are collected (no short-circuiting).

---

## Extension Methods for Reusable Rules

Create extension methods for common rule patterns:

```csharp
public static class StringSchemaExtensions
{
    public static StringContextlessSchema StartsWithUpper(this StringContextlessSchema schema)
    {
        return schema.Use(new StartsWithUpperRule());
    }

    public static StringContextlessSchema ContainsDigits(this StringContextlessSchema schema, int minDigits)
    {
        return schema.Use(new ContainsDigitsRule(minDigits));
    }
}

// Usage
Z.String()
    .StartsWithUpper()
    .ContainsDigits(2)
    .Email();
```

For context-aware schemas:

```csharp
public static class StringContextSchemaExtensions
{
    public static StringContextSchema<TContext> UniqueEmail<TContext>(
        this StringContextSchema<TContext> schema)
        where TContext : IEmailContext
    {
        return schema.Use(new UniqueEmailRule<TContext>());
    }
}

// Usage
Z.String()
    .Email()
    .Using<UserContext>()
    .UniqueEmail();
```

---

## Rule Best Practices

### Keep Rules Focused

Each rule should validate one thing:

```csharp
// Good - single responsibility
public sealed class MinDigitsRule : IValidationRule<string> { ... }
public sealed class MinUppercaseRule : IValidationRule<string> { ... }

// Avoid - multiple concerns
public sealed class PasswordComplexityRule : IValidationRule<string>
{
    // Checks digits, uppercase, lowercase, special chars all in one
}
```

### Use Meaningful Error Codes

Error codes should be machine-readable and consistent:

```csharp
// Good
new ValidationError(path, "min_digits", "Must contain at least 2 digits")
new ValidationError(path, "starts_uppercase", "Must start with uppercase")

// Avoid
new ValidationError(path, "error", "Invalid")
new ValidationError(path, "rule_failed", "Validation failed")
```

### Include Path in Errors

Always use `execution.Path` or `ctx.Execution.Path` for error paths:

```csharp
public ValidationError? Validate(string value, ValidationExecutionContext execution)
{
    return isValid
        ? null
        : new ValidationError(execution.Path, "code", "Message");
}
```

This ensures nested validation errors have correct paths like `user.email` or `items[0].name`.
