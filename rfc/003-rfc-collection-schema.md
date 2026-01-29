# RFC 003
## Revision of Collection schema

**Status:** ✅ Implemented

### Collection validation

- Length
- MinLength
- MaxLength

### Item validation
Attach a schema that each item will be validated with like:

```csharp
Z.Collection(
    Z.Int().Refine(x => x % 2 == 0, "Must be even")
)
.ValidateAsync(new[] { 1, 2, 3, 4 });
```

```csharp
var schema =
    Z.Object<User>()
        .Property(
            u => u.Roles,
            roles => roles.Each(item =>
                item.Refine(v => v == "Admin", "Must be Admin")
            )
        );

await schema.ValidateAsync(new User { Roles = ["Reader"] });
```