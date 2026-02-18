# RFC 002
## Rename methods to align with C# language

Field -> Property  (Align with)
Object -> Schema 


Example:
```csharp
var schema = Z.Schema<User>()
    .Property(u => u.Name, x => x.MinLength(3).MaxLength(50))
    .Property(u => u.Age, x => x.Minimum(0).Maximum(120));

public class User
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```