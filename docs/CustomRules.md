# Custom Rules

This guide covers custom validation with Zeta's public DSL: `Refine(...)`, `RefineAt(...)`, and `RefineAsync(...)`.

---

## Inline Refinements

Add custom logic with `.Refine(...)`:

```csharp
Z.String()
    .Refine(
        value => value.StartsWith("A"),
        message: "Must start with A",
        code: "starts_with_a"
    );
```

The `code` parameter is optional and defaults to `"custom_error"`.

---

## Target a Property Path with RefineAt

Use `.RefineAt(...)` on object schemas when the predicate needs the full object, but the error should land on a specific property path.

```csharp
Z.Schema<User>()
    .RefineAt(
        x => x.Email,
        x => x.Email.EndsWith("@company.com"),
        "Email must be company email",
        "company_email");
```

Context-aware with message factory:

```csharp
Z.Schema<GetOrCreateQuantificationQuery>()
    .Using<GetOrCreateQuantificationValidationContext>(BuildContextAsync)
    .RefineAt(
        x => x.PlanningCode,
        (_, ctx) => ctx.Errors.Count == 0,
        (_, ctx) => ctx.Errors.First().Message);
```

Use plain `.Refine(...)` when the error truly belongs at root (`$`).

---

## Context-Aware Refinements

After `.Using<TContext>()`, refinements can use context data:

```csharp
Z.String()
    .Using<UserContext>()
    .Refine(
        (value, ctx) => !ctx.BannedEmails.Contains(value),
        message: "Email is banned",
        code: "email_banned"
    );
```

You can mix value-only and value+context predicates:

```csharp
Z.String()
    .Using<UserContext>()
    .Refine(s => s.Length >= 3, "Too short")
    .Refine((s, ctx) => s != ctx.Reserved, "Reserved");
```

---

## Async Refinements

Use `RefineAsync` for async validation.

Context-aware:

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

Contextless (or value-only in context-aware schemas):

```csharp
Z.String()
    .RefineAsync(
        async (value, ct) => await SomeExternalValidationAsync(value, ct),
        message: "External validation failed",
        code: "external_invalid"
    );
```

---

## Reusable Custom Logic

For reusable logic, create extension methods that compose refinements:

```csharp
public static class StringSchemaExtensions
{
    public static StringContextlessSchema StartsWithUpper(this StringContextlessSchema schema)
        => schema.Refine(
            value => value.Length > 0 && char.IsUpper(value[0]),
            "Must start with uppercase",
            "starts_upper");

    public static StringContextlessSchema ContainsDigits(this StringContextlessSchema schema, int minDigits)
        => schema.Refine(
            value => value.Count(char.IsDigit) >= minDigits,
            $"Must contain at least {minDigits} digits",
            "min_digits");
}
```

Context-aware extension methods:

```csharp
public static class StringContextSchemaExtensions
{
    public static StringContextSchema<TContext> NotBanned<TContext>(
        this StringContextSchema<TContext> schema,
        Func<TContext, IReadOnlySet<string>> selector)
        => schema.Refine(
            (value, ctx) => !selector(ctx).Contains(value),
            "Value is banned",
            "banned_value");
}
```

---

## Best Practices

1. Keep each refinement focused on one rule.
2. Use stable, machine-readable error codes.
3. Always return path-aware errors by using `Refine`/`RefineAsync` instead of throwing exceptions.
4. Prefer extension methods when the same custom rule appears in multiple schemas.
