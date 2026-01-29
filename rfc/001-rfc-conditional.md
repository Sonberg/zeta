# RFC 002
## Improve mental model around Conditional Validation


# If
Rename "When" -> "If"

Can be added both on Object, Collections & Value types.
Check then condition (Both context-aware and contextless version):

**If** true: Use thenBranch with original schema

**else**: Use elseBranch with original schema

Conditional Validation**: Use `.If(...)` to create conditional branches based on the value or context:

### Contextless conditional

```csharp
 Z.Int()
     .If(
         condition: v => v >= 18,
         thenBranch: s => s.Max(100),
         elseBranch: s => s.Min(0).Max(17)
     );
 
 Z.Object<User>()
     .If(
         condition: v => v.HasEmail,
         thenBranch: s => s.Property(x => x.Email).Email(),
         elseBranch: s => s.Property(x => x.Email).Nullable()
     );
 ```

### Context-aware conditional

 ```csharp
 Z.String()
     .WithContext<MyContextType>()
     .If(
         condition: (v, ctx) => ctx.IsSpecialUser,
         thenBranch: s => s.MinLength(10),
         elseBranch: s => s.MinLength(5)
     );
 
```

Add example for collection and object

# Switch

Use `.Switch(...)` to create multiple conditional branches based on the value or context:

```csharp

Z .Collection<IAnimal>()
.ForEach(x => x.Switch(s => 
        s.Case(
        condition: (animal, ctx) => animal.Type == "Dog",
        branch: dogSchema => dogSchema
            .Field(a => a.BarkVolume, s => s.Min(0).Max(100))
    )
    .Case(
        condition: (animal, ctx) => animal.Type == "Cat",
        branch: catSchema => catSchema
            .Field(a => a.ClawSharpness, s => s.Min(0).Max(10))
    )
    .Default(
        defaultSchema => defaultSchema
            .Field(a => a.Name, s => s.MinLength(1))
    )
));

```
