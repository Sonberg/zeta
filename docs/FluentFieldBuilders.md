# Fluent Field Builders

Fluent field builders are the **default and recommended way** to define field validation in Zeta. This guide covers when and how to use them effectively.

---

## What Are Fluent Field Builders?

Fluent field builders allow you to define field schemas inline using a builder function. This is the most concise and readable approach for most validation scenarios.

### Default Approach (Fluent Builders)

```csharp
var userSchema = Z.Object<User>()
    .Field(u => u.Email, s => s.Email().MinLength(5))
    .Field(u => u.Age, s => s.Min(18).Max(100));
```

### For Composability (Pre-Built Schemas)

When you need to reuse the same schema across multiple objects, extract it:

```csharp
var emailSchema = Z.String().Email().MinLength(5);
var ageSchema = Z.Int().Min(18).Max(100);

var userSchema = Z.Object<User>()
    .Field(u => u.Email, emailSchema)
    .Field(u => u.Age, ageSchema);
```

---

## Supported Types

Fluent field builders are available for these primitive types:

- `string`
- `int`
- `double`
- `decimal`
- `bool`
- `Guid`
- `DateTime`
- `DateOnly`
- `TimeOnly`

---

## Examples by Type

### String Fields

```csharp
Z.Object<User>()
    .Field(u => u.Email, s => s.Email().MinLength(5).MaxLength(100))
    .Field(u => u.Username, s => s.MinLength(3).Alphanumeric())
    .Field(u => u.Website, s => s.Url().Nullable())
    .Field(u => u.PhoneNumber, s => s.Regex(@"^\d{3}-\d{3}-\d{4}$"));
```

### Numeric Fields

```csharp
Z.Object<Product>()
    .Field(p => p.Price, s => s.Positive().Precision(2))
    .Field(p => p.Stock, s => s.Min(0).Max(10000))
    .Field(p => p.Weight, s => s.Positive().Finite())
    .Field(p => p.Rating, s => s.Min(1.0).Max(5.0));
```

### Date and Time Fields

```csharp
Z.Object<Event>()
    .Field(e => e.StartDate, s => s.Future().WithinDays(90))
    .Field(e => e.BirthDate, s => s.Past().MinAge(18))
    .Field(e => e.EventDate, s => s.Weekday().Between(minDate, maxDate))
    .Field(e => e.StartTime, s => s.BusinessHours().After(minTime));
```

### Other Types

```csharp
Z.Object<Settings>()
    .Field(s => s.UserId, s => s.NotEmpty().Version(4))
    .Field(s => s.IsEnabled, s => s.IsTrue())
    .Field(s => s.AcceptedTerms, s => s.IsTrue());
```

---

## When to Use Each Approach

### Use Fluent Builders (Default)

This is the **recommended approach for most scenarios**:

1. **Single-use validation logic**
   ```csharp
   Z.Object<UserRequest>()
       .Field(u => u.Email, s => s.Email())
       .Field(u => u.Age, s => s.Min(18));
   ```

2. **Inline definitions for clarity**
   ```csharp
   // Clear and concise - validation logic right where you need it
   Z.Object<Product>()
       .Field(p => p.Name, s => s.MinLength(3).MaxLength(100))
       .Field(p => p.Price, s => s.Positive().Precision(2));
   ```

3. **Straightforward validation**
   ```csharp
   Z.Object<Comment>()
       .Field(c => c.Content, s => s.MinLength(1).MaxLength(500))
       .Field(c => c.CreatedAt, s => s.Past());
   ```

### Use Pre-Built Schemas (For Composability)

1. **Schema is reused across multiple objects**
   ```csharp
   var emailSchema = Z.String().Email().MinLength(5).MaxLength(100);

   var userSchema = Z.Object<User>()
       .Field(u => u.Email, emailSchema);

   var accountSchema = Z.Object<Account>()
       .Field(a => a.Email, emailSchema);  // Reused
   ```

2. **Schema requires custom refinements**
   ```csharp
   var passwordSchema = Z.String()
       .MinLength(8)
       .Refine(p => p.Any(char.IsDigit), "Must contain digit")
       .Refine(p => p.Any(char.IsUpper), "Must contain uppercase");

   var userSchema = Z.Object<User>()
       .Field(u => u.Password, passwordSchema);
   ```

3. **Schema needs context awareness**
   ```csharp
   var emailSchema = Z.String()
       .Email()
       .Using<UserContext>()
       .Refine((email, ctx) => !ctx.EmailExists, "Email taken");

   var userSchema = Z.Object<User>()
       .Using<UserContext>()
       .Field(u => u.Email, emailSchema); // No cast needed
   ```

4. **Schema is complex with nested objects or arrays**
   ```csharp
   var addressSchema = Z.Object<Address>()
       .Field(a => a.Street, Z.String().MinLength(3))
       .Field(a => a.ZipCode, Z.String().Regex(@"^\d{5}$"));

   var userSchema = Z.Object<User>()
       .Field(u => u.Address, addressSchema);
   ```

---

## Context-Aware Field Builders

For context-aware schemas, you can still use fluent builders for primitive fields:

```csharp
var userSchema = Z.Object<User>()
    .Field(u => u.Name, s => s.MinLength(3))
    .Using<UserContext>()
    .Field(u => u.Email, s => s.Email().MinLength(5))  // Still works
    .Field(u => u.Username,
        Z.String()
            .MinLength(3)
            .Using<UserContext>()
            .RefineAsync(async (username, ctx, ct) =>
                !await ctx.Repo.UsernameExistsAsync(username, ct),
                "Username taken"));
```

**Note**: Fluent builders create contextless schemas. For context-aware validation, use pre-built schemas with `.Using<TContext>()`.

---

## Mixing Both Approaches

You can freely mix fluent builders and pre-built schemas:

```csharp
var addressSchema = Z.Object<Address>()
    .Field(a => a.Street, Z.String().MinLength(3))
    .Field(a => a.ZipCode, Z.String().Regex(@"^\d{5}$"));

var userSchema = Z.Object<User>()
    .Field(u => u.Email, s => s.Email().MinLength(5))      // Fluent
    .Field(u => u.Age, s => s.Min(18).Max(120))            // Fluent
    .Field(u => u.Address, addressSchema);                 // Pre-built
```

---

## Nullable Fields

### Nullable Value Types (`int?`, `double?`, etc.)

Nullable value type properties work directly — null values skip validation, non-null values are validated. No `.Nullable()` needed:

```csharp
Z.Object<User>()
    .Field(u => u.Age, s => s.Min(0).Max(120))              // int? — null skips validation
    .Field(u => u.Balance, s => s.Positive().Precision(2))   // decimal? — null skips validation
    .Field(u => u.BirthDate, s => s.Past().MinAge(18));      // DateTime? — null skips validation
```

This works with both inline builders and pre-built schemas:

```csharp
var ageSchema = Z.Int().Min(0).Max(120);
Z.Object<User>()
    .Field(u => u.Age, ageSchema);  // int? — null skips validation
```

### Nullable Reference Types (`string?`)

For nullable reference types, call `.Nullable()` on the schema if null should be a valid value:

```csharp
Z.Object<User>()
    .Field(u => u.MiddleName, s => s.MinLength(1).MaxLength(50).Nullable())  // string? — .Nullable() allows null
    .Field(u => u.Bio, s => s.MaxLength(500).Nullable());                    // string? — .Nullable() allows null
```

Without `.Nullable()`, a null `string?` field produces a `null_value` validation error.

---

## Best Practices

1. **Fluent builders first** - Use inline builders as your default approach
2. **Extract for reuse** - Only create pre-built schemas when the same validation is used in multiple places
3. **Be consistent** - Within a single schema, prefer one style over mixing both
4. **Composability when needed** - Pre-build schemas for nested objects, arrays, or context-aware validation

---

## Complete Example

```csharp
public sealed class CreateUserRequest
{
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public int Age { get; init; }
    public decimal? Salary { get; init; }
    public DateTime BirthDate { get; init; }
    public Guid? ReferralCode { get; init; }
}

var schema = Z.Object<CreateUserRequest>()
    .Field(u => u.Email, s => s
        .Email()
        .MinLength(5)
        .MaxLength(100))
    .Field(u => u.Username, s => s
        .MinLength(3)
        .MaxLength(20)
        .Alphanumeric())
    .Field(u => u.Age, s => s
        .Min(18)
        .Max(120))
    .Field(u => u.Salary, s => s              // decimal? — null skips validation
        .Positive()
        .Precision(2))
    .Field(u => u.BirthDate, s => s
        .Past()
        .MinAge(18))
    .Field(u => u.ReferralCode, s => s       // Guid? — null skips validation
        .NotEmpty()
        .Version(4));
```

---

## See Also

- [Validation Context](ValidationContext.md) - Context-aware schemas and async data loading
- [Custom Rules](CustomRules.md) - Creating reusable validation rules
