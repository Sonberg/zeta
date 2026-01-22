using Zeta.Schemas;

namespace Zeta;

public static class Zeta
{
    public static StringSchema String() => new();
    public static StringSchema<TContext> String<TContext>() => new();

    public static IntSchema Int() => new();
    public static IntSchema<TContext> Int<TContext>() => new();

    public static ObjectSchema<T> Object<T>() => new();
    public static ObjectSchema<T, TContext> Object<T, TContext>() => new();
}
