namespace Zeta.SourceGenerators;

/// <summary>
/// Defines mappings between C# types and their corresponding schema classes.
/// </summary>
internal static class SchemaMapping
{
    internal record Mapping(string Type, string SchemaClass, string ContextSchemaClass, string FactoryMethod, bool IsValueType);

    /// <summary>
    /// Core primitive types that have schema implementations.
    /// Available in all target frameworks including .NET Standard 2.0.
    /// </summary>
    internal static readonly Mapping[] PrimitiveMappings =
    {
        new("string", "StringContextlessSchema", "StringContextSchema", "Z.String", false),
        new("int", "IntContextlessSchema", "IntContextSchema", "Z.Int", true),
        new("double", "DoubleContextlessSchema", "DoubleContextSchema", "Z.Double", true),
        new("decimal", "DecimalContextlessSchema", "DecimalContextSchema", "Z.Decimal", true),
        new("bool", "BoolContextlessSchema", "BoolContextSchema", "Z.Bool", true),
        new("System.Guid", "GuidContextlessSchema", "GuidContextSchema", "Z.Guid", true),
        new("System.DateTime", "DateTimeContextlessSchema", "DateTimeContextSchema", "Z.DateTime", true)
    };

    /// <summary>
    /// Types that are only available in .NET 6+ (not in .NET Standard 2.0).
    /// These require #if !NETSTANDARD2_0 conditional compilation.
    /// </summary>
    internal static readonly Mapping[] ModernNetMappings =
    {
        new("DateOnly", "DateOnlyContextlessSchema", "DateOnlyContextSchema", "Z.DateOnly", true),
        new("TimeOnly", "TimeOnlyContextlessSchema", "TimeOnlyContextSchema", "Z.TimeOnly", true)
    };
}
