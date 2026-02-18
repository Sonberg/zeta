# .Each() with Object Builders

Support for inline object builders in `.Each()` allows you to validate complex object collections without pre-building schemas.

## Before (Pre-building required)

```csharp
// Had to pre-build the schema first
var orderItemSchema = Z.Schema<OrderItemDto>()
    .Property(i => i.ProductId, Z.Guid())
    .Property(i => i.Quantity, Z.Int().Min(1).Max(100))
    .Property(i => i.Notes, Z.String().MaxLength(500).Nullable());

// Then pass it to Z.Collection()
var orderSchema = Z.Schema<CreateOrderRequest>()
    .Property(o => o.CustomerId, Z.Guid())
    .Property(o => o.Items, Z.Collection(orderItemSchema))
    .Property(o => o.ShippingAddress, addressSchema);
```

## After (Inline object builders)

```csharp
// Build the schema inline with .Each()
var orderSchema = Z.Schema<CreateOrderRequest>()
    .Property(o => o.CustomerId, Z.Guid())
    .Property(o => o.Items, Z.Collection<OrderItemDto>()
        .Each(item => item
            .Property(i => i.ProductId, Z.Guid())
            .Property(i => i.Quantity, Z.Int().Min(1).Max(100))
            .Property(i => i.Notes, Z.String().MaxLength(500).Nullable()))
        .MinLength(1)
        .MaxLength(50))
    .Property(o => o.ShippingAddress, addressSchema);
```

## Usage

The `.Each()` method with object builders works for both contextless and context-aware schemas:

### Contextless

```csharp
var schema = Z.Collection<Product>()
    .Each(p => p
        .Property(x => x.Name, Z.String().MinLength(3))
        .Property(x => x.Price, Z.Decimal().Min(0.01m)))
    .MinLength(1);
```

### Context-aware

```csharp
var schema = Z.Collection<Product>()
    .Each(p => p
        .Property(x => x.Name, Z.String().MinLength(3))
        .Property(x => x.Sku, Z.String())
        .Using<ProductContext>()
        .Refine((product, ctx) => !ctx.SkuExists(product.Sku), "SKU exists"))
    .MinLength(1);
```

## Benefits

1. **More concise** - No need to declare separate schema variables
2. **Better readability** - Schema definition is inline with usage
3. **Consistent API** - Same pattern as simple types (`.Each(s => s.Email())`)
4. **Still composable** - Pre-built schemas still work when you need reusability

## When to use which approach?

**Use inline builders (`.Each()`):**
- Schema is used only once
- Simple validation logic
- Want concise, inline definitions

**Use pre-built schemas:**
- Schema is reused across multiple places
- Complex validation logic that benefits from extraction
- Want to test the schema independently
