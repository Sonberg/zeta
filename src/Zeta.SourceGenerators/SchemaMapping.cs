namespace Zeta.SourceGenerators;

/// <summary>
/// Defines mappings between C# types and their corresponding schema classes.
/// </summary>
internal static class SchemaMapping
{
    internal record Mapping(string Type, string SchemaClass, string FactoryMethod, bool IsValueType);

    /// <summary>
    /// Core primitive types that have schema implementations.
    /// Available in all target frameworks including .NET Standard 2.0.
    /// </summary>
    internal static readonly Mapping[] PrimitiveMappings =
    {
        new("string", "StringContextlessSchema", "Z.String", false),
        new("int", "IntContextlessSchema", "Z.Int", true),
        new("double", "DoubleContextlessSchema", "Z.Double", true),
        new("decimal", "DecimalContextlessSchema", "Z.Decimal", true),
        new("bool", "BoolContextlessSchema", "Z.Bool", true),
        new("System.Guid", "GuidContextlessSchema", "Z.Guid", true),
        new("System.DateTime", "DateTimeContextlessSchema", "Z.DateTime", true)
    };

    /// <summary>
    /// Types that are only available in .NET 6+ (not in .NET Standard 2.0).
    /// These require #if !NETSTANDARD2_0 conditional compilation.
    /// </summary>
    internal static readonly Mapping[] ModernNetMappings =
    {
        new("DateOnly", "DateOnlyContextlessSchema", "Z.DateOnly", true),
        new("TimeOnly", "TimeOnlyContextlessSchema", "Z.TimeOnly", true)
    };
}
