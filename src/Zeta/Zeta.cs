using Zeta.Core;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Entry point for creating Zeta schemas.
/// </summary>
public static class Z
{
    // String
    public static StringContextlessSchema String() => new();

    // Integer
    public static IntContextlessSchema Int() => new();

    // Double
    public static DoubleContextlessSchema Double() => new();

    // Decimal
    public static DecimalContextlessSchema Decimal() => new();

    // Object
    public static ObjectContextlessSchema<T> Object<T>() where T : class => new();

    // Collection
    public static CollectionContextlessSchema<TElement> Collection<TElement>() => new(null, new ContextlessRuleEngine<ICollection<TElement>>());

    // Collection with pre-built element schema (for nested objects)
    public static CollectionContextlessSchema<TElement> Collection<TElement>(ISchema<TElement> elementSchema) => new(elementSchema, new ContextlessRuleEngine<ICollection<TElement>>());
    
    // DateTime
    public static DateTimeContextlessSchema DateTime() => new();

#if !NETSTANDARD2_0
    // DateOnly
    public static DateOnlyContextlessSchema DateOnly() => new();

    // TimeOnly
    public static TimeOnlyContextlessSchema TimeOnly() => new();
#endif

    // Guid
    public static GuidContextlessSchema Guid() => new();

    // Bool
    public static BoolContextlessSchema Bool() => new();

    public static ValidationContext Context() => ValidationContext.Empty;

    public static ValidationContext<TContext> Context<TContext>(TContext value) => new(value);
}