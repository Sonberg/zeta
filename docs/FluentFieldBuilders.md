# Fluent Property Builders

Fluent property builders are the **default and recommended way** to define property validation in Zeta. This guide covers when and how to use them effectively.

---

## What Are Fluent Property Builders?

Fluent property builders allow you to define property schemas inline using a builder function. This is the most concise and readable approach for most validation scenarios.

### Default Approach (Fluent Builders)

```csharp
var userSchema = Z.Schema<User>()
    .Property(u => u.Email, s => s.Email().MinLength(5))
    .Property(u => u.Age, s => s.Min(18).Max(100));
```

### For Composability (Pre-Built Schemas)

When you need to reuse the same schema across multiple objects, extract it:

```csharp
var emailSchema = Z.String().Email().MinLength(5);
var ageSchema = Z.Int().Min(18).Max(100);

var userSchema = Z.Schema<User>()
    .Property(u => u.Email, emailSchema)
    .Property(u => u.Age, ageSchema);
```

---

## Supported Types

Fluent property builders are available for these primitive types:

- `string`
- `int`
- `double`
- `decimal`
- `bool`
- `Guid`
- `DateTime`
- `DateOnly`
- `TimeOnly`
- `enum` (via `Z.Enum<TEnum>()`)

---

## Examples by Type

### String Fields

```csharp
Z.Schema<User>()
    .Property(u => u.Email, s => s.Email().MinLength(5).MaxLength(100))
    .Property(u => u.Username, s => s.MinLength(3).Alphanumeric())
    .Property(u => u.Website, s => s.Url().Nullable())
    .Property(u => u.PhoneNumber, s => s.Regex(@"^\d{3}-\d{3}-\d{4}$"));
```

### Numeric Fields

```csharp
Z.Schema<Product>()
    .Property(p => p.Price, s => s.Positive().Precision(2))
    .Property(p => p.Stock, s => s.Min(0).Max(10000))
    .Property(p => p.Weight, s => s.Positive().Finite())
    .Property(p => p.Rating, s => s.Min(1.0).Max(5.0));
```

### Date and Time Fields

```csharp
Z.Schema<Event>()
    .Property(e => e.StartDate, s => s.Future().WithinDays(90))
    .Property(e => e.BirthDate, s => s.Past().MinAge(18))
    .Property(e => e.EventDate, s => s.Weekday().Between(minDate, maxDate))
    .Property(e => e.StartTime, s => s.BusinessHours().Min(minTime));
```

### Other Types

```csharp
Z.Schema<Settings>()
    .Property(s => s.UserId, s => s.NotEmpty().Version(4))
    .Property(s => s.IsEnabled, s => s.IsTrue())
    .Property(s => s.AcceptedTerms, s => s.IsTrue())
    .Property(s => s.Channel, s => s.Defined());
```

---

## When to Use Each Approach

### Use Fluent Builders (Default)

This is the **recommended approach for most scenarios**:

1. **Single-use validation logic**
   ```csharp
   Z.Schema<UserRequest>()
       .Property(u => u.Email, s => s.Email())
       .Property(u => u.Age, s => s.Min(18));
   ```

2. **Inline definitions for clarity**
   ```csharp
   // Clear and concise - validation logic right where you need it
   Z.Schema<Product>()
       .Property(p => p.Name, s => s.MinLength(3).MaxLength(100))
       .Property(p => p.Price, s => s.Positive().Precision(2));
   ```

3. **Straightforward validation**
   ```csharp
   Z.Schema<Comment>()
       .Property(c => c.Content, s => s.MinLength(1).MaxLength(500))
       .Property(c => c.CreatedAt, s => s.Past());
   ```

### Use Pre-Built Schemas (For Composability)

1. **Schema is reused across multiple objects**
   ```csharp
   var emailSchema = Z.String().Email().MinLength(5).MaxLength(100);

   var userSchema = Z.Schema<User>()
       .Property(u => u.Email, emailSchema);

   var accountSchema = Z.Schema<Account>()
       .Property(a => a.Email, emailSchema);  // Reused
   ```

2. **Schema requires custom refinements**
   ```csharp
   var passwordSchema = Z.String()
       .MinLength(8)
       .Refine(p => p.Any(char.IsDigit), "Must contain digit")
       .Refine(p => p.Any(char.IsUpper), "Must contain uppercase");

   var userSchema = Z.Schema<User>()
       .Property(u => u.Password, passwordSchema);
   ```

3. **Schema needs context awareness**
   ```csharp
   var emailSchema = Z.String()
       .Email()
       .Using<UserContext>()
       .Refine((email, ctx) => !ctx.EmailExists, "Email taken");

   var userSchema = Z.Schema<User>()
       .Using<UserContext>()
       .Property(u => u.Email, emailSchema); // No cast needed
   ```

4. **Schema is complex with nested objects or arrays**
   ```csharp
   var addressSchema = Z.Schema<Address>()
       .Property(a => a.Street, Z.String().MinLength(3))
       .Property(a => a.ZipCode, Z.String().Regex(@"^\d{5}$"));

   var userSchema = Z.Schema<User>()
       .Property(u => u.Address, addressSchema);
   ```

---

## Context-Aware Property Builders

For context-aware schemas, you can still use fluent builders for primitive fields:

```csharp
var userSchema = Z.Schema<User>()
    .Property(u => u.Name, s => s.MinLength(3))
    .Using<UserContext>()
    .Property(u => u.Email, s => s.Email().MinLength(5))  // Still works
    .Property(u => u.Username,
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
var addressSchema = Z.Schema<Address>()
    .Property(a => a.Street, Z.String().MinLength(3))
    .Property(a => a.ZipCode, Z.String().Regex(@"^\d{5}$"));

var userSchema = Z.Schema<User>()
    .Property(u => u.Email, s => s.Email().MinLength(5))      // Fluent
    .Property(u => u.Age, s => s.Min(18).Max(120))            // Fluent
    .Property(u => u.Address, addressSchema);                 // Pre-built
```

---

## Nullable Fields

### Nullable Value Types (`int?`, `double?`, etc.)

Nullable value type properties work directly — null values skip validation, non-null values are validated. No `.Nullable()` needed:

```csharp
Z.Schema<User>()
    .Property(u => u.Age, s => s.Min(0).Max(120))              // int? — null skips validation
    .Property(u => u.Balance, s => s.Positive().Precision(2))   // decimal? — null skips validation
    .Property(u => u.BirthDate, s => s.Past().MinAge(18));      // DateTime? — null skips validation
```

This works with both inline builders and pre-built schemas:

```csharp
var ageSchema = Z.Int().Min(0).Max(120);
Z.Schema<User>()
    .Property(u => u.Age, ageSchema);  // int? — null skips validation
```

### Nullable Reference Types (`string?`)

For nullable reference types, call `.Nullable()` on the schema if null should be a valid value:

```csharp
Z.Schema<User>()
    .Property(u => u.MiddleName, s => s.MinLength(1).MaxLength(50).Nullable())  // string? — .Nullable() allows null
    .Property(u => u.Bio, s => s.MaxLength(500).Nullable());                    // string? — .Nullable() allows null
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

var schema = Z.Schema<CreateUserRequest>()
    .Property(u => u.Email, s => s
        .Email()
        .MinLength(5)
        .MaxLength(100))
    .Property(u => u.Username, s => s
        .MinLength(3)
        .MaxLength(20)
        .Alphanumeric())
    .Property(u => u.Age, s => s
        .Min(18)
        .Max(120))
    .Property(u => u.Salary, s => s              // decimal? — null skips validation
        .Positive()
        .Precision(2))
    .Property(u => u.BirthDate, s => s
        .Past()
        .MinAge(18))
    .Property(u => u.ReferralCode, s => s       // Guid? — null skips validation
        .NotEmpty()
        .Version(4));
```

---

## See Also

- [Validation Context](ValidationContext.md) - Context-aware schemas and async data loading
- [Custom Rules](CustomRules.md) - Creating reusable validation rules
