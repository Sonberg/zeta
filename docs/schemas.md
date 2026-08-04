# Schema types

Everything starts at the static `Z` entry point. Each factory returns an immutable schema you can
chain rules onto.

| Factory | Validates |
|---|---|
| `Z.String()` | `string` |
| `Z.Int()` | `int` |
| `Z.Double()` | `double` |
| `Z.Decimal()` | `decimal` |
| `Z.Bool()` | `bool` |
| `Z.Guid()` | `Guid` |
| `Z.DateTime()` | `DateTime` |
| `Z.DateOnly()` | `DateOnly` |
| `Z.TimeOnly()` | `TimeOnly` |
| `Z.Enum<TEnum>()` | any `enum` |
| `Z.Schema<T>()` | a reference type, property by property |
| `Z.Collection<TElement>()` | arrays and lists |
| `Z.Dictionary<TKey, TValue>()` | dictionaries |

For the complete list of rules available on each of these, see the
[validator reference](/validators).

## Value schemas

### String

```csharp
Z.String()
    .MinLength(3)
    .MaxLength(100)
    .Email()
    .NotEmpty();
```

### Numbers

```csharp
Z.Int().Min(0).Max(100);

Z.Double().Positive().Finite();

Z.Decimal()
    .Positive()
    .Precision(2)         // at most 2 decimal places
    .MultipleOf(0.25m);
```

### Dates and times

```csharp
Z.DateTime()
    .Past()
    .MinAge(18);          // for birthdates

Z.DateOnly().Between(min, max).Weekday();

Z.TimeOnly().BusinessHours();   // 9am–5pm by default
```

Date and time rules read "now" from the `TimeProvider` on the validation run, not from
`DateTime.UtcNow` directly — which is what makes them testable. See [Testing](/testing).

### Guid, bool, enum

```csharp
Z.Guid().NotEmpty().Version(4);

Z.Bool().IsTrue();                 // e.g. "terms accepted"

Z.Enum<Channel>()
    .Defined()                     // must be a declared member, not an arbitrary cast int
    .OneOf(Channel.Online, Channel.Store);
```

`.Defined()` matters more than it looks: `(Channel)99` is a perfectly legal value of an enum type in
C#, so an unchecked cast from user input sails straight through the type system.

## Object schemas

`Z.Schema<T>()` validates a reference type property by property.

```csharp
Z.Schema<User>()
    .Property(u => u.Name, s => s.MinLength(2))
    .Property(u => u.Email, s => s.Email().MinLength(5))
    .Property(u => u.Age, s => s.Min(18).Max(100))
    .Refine(u => u.Password != u.Email, "Password cannot be email");
```

Inline builders (`s => s.MinLength(2)`) are available for every primitive type listed above. To
share a schema across several objects, build it separately and pass it in — see
[Fluent property builders](/property-builders) for when to prefer each style.

```csharp
Z.Schema<User>()
    .Property(u => u.Address, addressSchema);
```

### Attaching an object-level rule to one property

A rule sometimes needs the whole object but should report against a single field. `.RefineAt()`
takes an extra expression naming where the error lands:

```csharp
Z.Schema<User>()
    .Property(u => u.Email, s => s.Email())
    .RefineAt(
        u => u.Email,                                 // error path: $.email
        u => u.Email != "blocked@company.com",        // predicate over the whole object
        "Email is blocked");
```

Without `.RefineAt()`, a `.Refine()` on an object schema reports at the object's own path (`$`),
which is correct for genuinely cross-field rules but unhelpful for a client trying to highlight a
form field.

### Validation order

Object schemas run their stages in a fixed order:

1. **Properties** — each `.Property()` schema
2. **Type assertions** — `.As<T>()`
3. **Conditionals** — `.If()`
4. **Rules** — `.Refine()` / `.RefineAt()`

Every stage runs; nothing short-circuits on the first failure.

## Nullability

Schemas are required by default.

```csharp
Z.String().Nullable();
Z.Int().Nullable();
Z.Schema<Address>().Nullable();
```

Inside an object schema the two kinds of nullable behave differently:

```csharp
public record User(string Name, int? Age, decimal? Balance, string? Bio);

Z.Schema<User>()
    .Property(u => u.Name, s => s.MinLength(2))
    .Property(u => u.Age, s => s.Min(0).Max(120))              // int? — null skips the rules
    .Property(u => u.Balance, s => s.Positive().Precision(2))  // decimal? — null skips the rules
    .Property(u => u.Bio, s => s.MaxLength(500).Nullable());   // string? — needs .Nullable()
```

Nullable value types are detectable at runtime (`Nullable<T>` is a real type), so Zeta handles them
for you. Nullable reference type annotations are erased at runtime, so `string?` is indistinguishable
from `string` and you have to say what you mean.

A null value that isn't allowed produces the error code `null_value`.

::: tip
`ISchema<T>` is always non-nullable in its type parameter — there is no `ISchema<int?>`. Nullability
is a property of the schema (`AllowNull`), not of its type argument.
:::

## Collections and dictionaries

```csharp
Z.Collection<string>()
    .Each(s => s.Email())      // per element
    .MinLength(1)              // on the collection itself
    .MaxLength(10);
```

```csharp
Z.Dictionary<string, int>()
    .EachKey(Z.String().MinLength(1))
    .EachValue(Z.Int().Min(0));
```

Errors carry the index or key: `$.tags[0]`, `$.scores['alice']`. Full detail in
[Collections](/collections) and [Dictionaries](/dictionaries).

## Conditionals and polymorphism

`.If()` applies rules only when a predicate holds, and doubles as the branch mechanism for
polymorphic types:

```csharp
Z.Schema<Order>()
    .If(o => o.PaymentMethod == "card", s => s
        .Property(o => o.CardNumber, n => n.MinLength(16)));
```

See [Conditionals and polymorphism](/conditionals).

## Context-aware schemas

Any schema can be promoted with `.Using<TContext>()` so its rules receive a context object — the
place to put data loaded asynchronously from a database or service:

```csharp
Z.String().Email()
    .Using<UserContext>()
    .RefineAsync(
        async (email, ctx, ct) => !await ctx.Repo.EmailExistsAsync(email, ct),
        "Email already taken",
        "email_exists");
```

Rules, properties and conditionals added before the promotion transfer automatically. This is the
subject of [Context-aware validation](/validation-run).
