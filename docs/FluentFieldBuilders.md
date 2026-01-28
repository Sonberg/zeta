# Fluent Field Builders

This guide covers Zeta's fluent field builder pattern for defining inline schemas in object validation.

---

## What Are Fluent Field Builders?

Fluent field builders allow you to define field schemas inline using a builder function instead of pre-building schemas. This makes object schema definitions more concise and readable.

### Traditional Syntax

```csharp
var emailSchema = Z.String().Email().MinLength(5);
var ageSchema = Z.Int().Min(18).Max(100);

var userSchema = Z.Object<User>()
    .Field(u => u.Email, emailSchema)
    .Field(u => u.Age, ageSchema);
```

### Fluent Builder Syntax

```csharp
var userSchema = Z.Object<User>()
    .Field(u => u.Email, s => s.Email().MinLength(5))
    .Field(u => u.Age, s => s.Min(18).Max(100));
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

### Use Fluent Builders When:

1. **Schema is simple and used once**
   ```csharp
   Z.Object<UserRequest>()
       .Field(u => u.Email, s => s.Email())
       .Field(u => u.Age, s => s.Min(18));
   ```

2. **Inline definition improves readability**
   ```csharp
   // Clear and concise
   Z.Object<Product>()
       .Field(p => p.Name, s => s.MinLength(3).MaxLength(100))
       .Field(p => p.Price, s => s.Positive().Precision(2));
   ```

3. **Validation logic is straightforward**
   ```csharp
   Z.Object<Comment>()
       .Field(c => c.Content, s => s.MinLength(1).MaxLength(500))
       .Field(c => c.CreatedAt, s => s.Past());
   ```

### Use Pre-Built Schemas When:

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
       .WithContext<UserContext>()
       .Refine((email, ctx) => !ctx.EmailExists, "Email taken");

   var userSchema = Z.Object<User>()
       .WithContext<User, UserContext>()
       .Field(u => u.Email, emailSchema);
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
    .WithContext<User, UserContext>()
    .Field(u => u.Email, s => s.Email().MinLength(5))  // Still works
    .Field(u => u.Username,
        Z.String()
            .MinLength(3)
            .WithContext<UserContext>()
            .RefineAsync(async (username, ctx, ct) =>
                !await ctx.Repo.UsernameExistsAsync(username, ct),
                "Username taken"));
```

**Note**: Fluent builders create contextless schemas. For context-aware validation, use pre-built schemas with `.WithContext<TContext>()`.

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

Fluent builders work with nullable types:

```csharp
Z.Object<User>()
    .Field(u => u.MiddleName, s => s.MinLength(1).MaxLength(50).Nullable())
    .Field(u => u.Age, s => s.Min(0).Max(120).Nullable());
```

**Note**: Call `.Nullable()` at the end of the fluent chain.

---

## Best Practices

1. **Keep it simple** - Use fluent builders for straightforward validation
2. **Extract when reused** - Create named schemas when used in multiple places
3. **Be consistent** - Pick a style and stick with it within a schema
4. **Readable first** - Choose the approach that makes your code clearest

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
    .Field(u => u.Salary, s => s
        .Positive()
        .Precision(2)
        .Nullable())
    .Field(u => u.BirthDate, s => s
        .Past()
        .MinAge(18))
    .Field(u => u.ReferralCode, s => s
        .NotEmpty()
        .Version(4)
        .Nullable());
```

---

## See Also

- [Validation Context](ValidationContext.md) - Context-aware schemas and async data loading
- [Custom Rules](CustomRules.md) - Creating reusable validation rules
