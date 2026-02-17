# RFC: Immutable, Append-Only Schema Design in Zeta

## Status
Proposed

## Author
Zeta Core

## Motivation

The current schema implementation is **mutable**. Each DSL call (e.g. `.MinLength()`) mutates internal rule state and returns `this`.

While functional, mutation introduces:

- ❌ Hidden side-effects
- ❌ Risk when reusing schema instances
- ❌ Harder reasoning about schema composition
- ❌ Reduced thread-safety guarantees
- ❌ Less predictable behavior when branching chains

We want:

- ✅ Immutable schema chains
- ✅ Structural rule sharing
- ✅ Zero unnecessary allocations
- ✅ No performance regression
- ✅ Cleaner mental model

---

## Goals

1. Make every DSL step return a **new schema instance**
2. Keep rule addition **O(1)**
3. Avoid copying rule collections
4. Preserve current validation performance characteristics
5. Enable safe schema reuse and branching

---

## Non-Goals

- Changing public DSL surface
- Introducing functional programming complexity for consumers
- Increasing runtime allocations in hot paths

---

## Proposed Design

### 1. Immutable Schema Instances

Instead of:

```csharp
Use(new MinLengthRule(min));
return this;
```
We introduce:

```csharp
return Append(new MinLengthRule(min));
```

2. Append-Only Rule Engine

Replace mutable rule lists with a persistent linked structure:

```csharp
internal sealed class RuleNode<T>
{
public IValidationRule<T> Rule { get; }
public RuleNode<T>? Previous { get; }

    public RuleNode(IValidationRule<T> rule, RuleNode<T>? previous)
    {
        Rule = rule;
        Previous = previous;
    }
}
```

Rule engine:

```csharp
public sealed class ContextlessRuleEngine<T>
{
private readonly RuleNode<T>? _head;

    public ContextlessRuleEngine<T> Add(IValidationRule<T> rule)
        => new(new RuleNode<T>(rule, _head));
}
```

Characteristics:

O(1) append

No copying

Full structural sharing

Memory efficient

3. Updated Base Schema
```csharp
   public abstract class ContextlessSchema<T, TSchema>
   where TSchema : ContextlessSchema<T, TSchema>
   {
   protected readonly ContextlessRuleEngine<T> Rules;
   protected readonly bool AllowNull;

   protected TSchema Append(IValidationRule<T> rule)
   {
   var newRules = Rules.Add(rule);
   return CreateInstance(newRules, AllowNull);
   }

   protected abstract TSchema CreateInstance(
   ContextlessRuleEngine<T> rules,
   bool allowNull);
   }
```
4. Example: Immutable String Schema
```csharp
   public StringContextlessSchema MinLength(int min, string? message = null)
   => Append(new MinLengthRule(min, message));
```

Every call creates a new schema instance.

Behavioral Changes
Before (Mutable)
```csharp
var baseSchema = Z.String().MinLength(5);
var adminSchema = baseSchema.MaxLength(100);

// baseSchema now also has MaxLength(100) ❌
```
After (Immutable)
```csharp
var baseSchema = Z.String().MinLength(5);
var adminSchema = baseSchema.MaxLength(100);

// baseSchema unaffected ✅
```

Validate so this is should sucess;
```csharp
[Fact]
public async Task As_TypeMismatch_ReturnsTypeMismatchError()
{
var schema = Z.Object<IAnimal>();
schema.As<Dog>();

        var result = await schema.ValidateAsync(new Cat(5));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "type_mismatch");
    }
```
