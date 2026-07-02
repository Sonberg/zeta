# RFC 003

## Revision of Collection schema

**Status:** ✅ Implemented

### Collection validation

- Length
- MinLength
- MaxLength

## Item validation

Attach a schema that each item will be validated with like:

.Each only accept ISchema<T> or ISchema<T, TContext>

### Out of scope

```csharp
Z.Collection(
    Z.Int().Refine(x => x % 2 == 0, "Must be even")
)
.ValidateAsync(new[] { 1, 2, 3, 4 });
```

```csharp
var schema =
    Z.Schema<User>()
        .Property(
            u => u.Roles,
            roles => roles.Each(item =>
                item.Refine(v => v == "Admin", "Must be Admin")
            )
        );

await schema.ValidateAsync(new User { Roles = ["Reader"] });
```

Make this work:

```csharp
var schema = Z.Schema<CreateOrderRequest>()
    .WithContext<CreateOrderContext>()
    .Property(x => x.CustomerId, x => x.NotEmpty())
    .Property(x => x.Items, x => x.Each(item => item
        .Property(i => i.ProductId, Z.Guid())
        .Property(i => i.Quantity, Z.Int().Min(1).Max(100))));
```
