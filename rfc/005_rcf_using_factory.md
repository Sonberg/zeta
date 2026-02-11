
## Remove IContextFactory 
It adds complexity and is not necessary. Instead, we can directly use the context type in the schema definition and validation.

## Rename WithContext -> Using<Context>()
Optional factory method

Example:

```csharp
var schema = Z.Object<CreateOrderRequest>()
    .Using<CreateOrderContext>(async (value, sp) => new CreateOrderContext { HasAccess = await sp.GetService<IPermissionService>().CheckAccessAsync(value.CustomerId) })
    .Field(x => x.CustomerId, x => x.NotEmpty())
    .Field(x => x.Items, x => x.Each(item => item
        .Field(i => i.ProductId, Z.Guid())
        .Field(i => i.Quantity, Z.Int().Min(1).Max(100))));
```

In IZetaValidator:

Request context must be passed in directly or One factory method added. The factory method is saved in Optional field in ContextSchema and located then when validation is called. If factory method is not provided, context is created with default constructor. 

