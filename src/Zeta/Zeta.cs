using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Entry point for creating Zeta schemas.
/// </summary>
public static class Z
{
    // String
    public static StringSchema String() => new();

    // Integer
    public static IntSchema Int() => new();

    // Double
    public static DoubleSchema Double() => new();

    // Decimal
    public static DecimalSchema Decimal() => new();

    // Object
    public static ObjectSchema<T> Object<T>() => new();

    // Array
    public static ArraySchema<TElement> Array<TElement>(ISchema<TElement> elementSchema) => new(elementSchema);
    public static ArraySchema<TElement> Array<TElement>(ISchema<TElement, object?> elementSchema) => new(elementSchema);

    // List
    public static ListSchema<TElement> List<TElement>(ISchema<TElement> elementSchema) => new(elementSchema);
    public static ListSchema<TElement> List<TElement>(ISchema<TElement, object?> elementSchema) => new(elementSchema);

    // DateTime
    public static DateTimeSchema DateTime() => new();

#if !NETSTANDARD2_0
    // DateOnly
    public static DateOnlySchema DateOnly() => new();

    // TimeOnly
    public static TimeOnlySchema TimeOnly() => new();
#endif

    // Guid
    public static GuidSchema Guid() => new();

    // Bool
    public static BoolSchema Bool() => new();
}
