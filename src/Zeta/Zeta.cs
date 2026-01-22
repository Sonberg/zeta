using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Entry point for creating Zeta validation schemas.
/// </summary>
public static class Zeta
{
    /// <summary>
    /// Creates a schema for validating strings.
    /// </summary>
    public static StringSchema String() => new();

    /// <summary>
    /// Creates a schema for validating integers.
    /// </summary>
    public static IntSchema Int() => new();

    /// <summary>
    /// Creates a schema for validating objects of type T.
    /// </summary>
    public static ObjectSchema<T> Object<T>() => new();
}
