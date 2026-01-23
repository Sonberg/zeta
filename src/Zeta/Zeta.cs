using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Entry point for creating Zeta schemas.
/// </summary>
public static class Z
{
    public static StringSchema String() => new();
    public static StringSchema<TContext> String<TContext>() => new();

    public static IntSchema Int() => new();
    public static IntSchema<TContext> Int<TContext>() => new();

    public static ObjectSchema<T> Object<T>() => new();
    public static ObjectSchema<T, TContext> Object<T, TContext>() => new();
}
