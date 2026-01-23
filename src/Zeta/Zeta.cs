using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Entry point for creating Zeta schemas.
/// </summary>
public static class Z
{
    // String
    public static StringSchema String() => new();
    public static StringSchema<TContext> String<TContext>() => new();

    // Integer
    public static IntSchema Int() => new();
    public static IntSchema<TContext> Int<TContext>() => new();

    // Double
    public static DoubleSchema Double() => new();
    public static DoubleSchema<TContext> Double<TContext>() => new();

    // Decimal
    public static DecimalSchema Decimal() => new();
    public static DecimalSchema<TContext> Decimal<TContext>() => new();

    // Object
    public static ObjectSchema<T> Object<T>() => new();
    public static ObjectSchema<T, TContext> Object<T, TContext>() => new();

    // Array
    public static ArraySchema<TElement> Array<TElement>(ISchema<TElement> elementSchema) => new(elementSchema);
    public static ArraySchema<TElement, TContext> Array<TElement, TContext>(ISchema<TElement, TContext> elementSchema) => new(elementSchema);

    // List
    public static ListSchema<TElement> List<TElement>(ISchema<TElement> elementSchema) => new(elementSchema);
    public static ListSchema<TElement, TContext> List<TElement, TContext>(ISchema<TElement, TContext> elementSchema) => new(elementSchema);

    // DateTime
    public static DateTimeSchema DateTime() => new();
    public static DateTimeSchema<TContext> DateTime<TContext>() => new();

    // DateOnly
    public static DateOnlySchema DateOnly() => new();
    public static DateOnlySchema<TContext> DateOnly<TContext>() => new();

    // TimeOnly
    public static TimeOnlySchema TimeOnly() => new();
    public static TimeOnlySchema<TContext> TimeOnly<TContext>() => new();

    // Guid
    public static GuidSchema Guid() => new();
    public static GuidSchema<TContext> Guid<TContext>() => new();

    // Bool
    public static BoolSchema Bool() => new();
    public static BoolSchema<TContext> Bool<TContext>() => new();
}
