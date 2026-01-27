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

    // Array
    public static ArrayContextlessSchema<TElement> Array<TElement>(ISchema<TElement> elementSchema) => new(elementSchema, new ContextlessRuleEngine<TElement[]>());

    // List
    public static ListContextlessSchema<TElement> List<TElement>(ISchema<TElement> elementSchema) => new(elementSchema);

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
}
