namespace Zeta.SourceGenerators;

/// <summary>
/// Defines mappings between C# types and their corresponding schema classes.
/// </summary>
internal static class SchemaMapping
{
    internal record Mapping(string Type, string SchemaClass, string ContextSchemaClass, string FactoryMethod, bool IsValueType);

    /// <summary>
    /// Core primitive types that have schema implementations.
    /// </summary>
    internal static readonly Mapping[] PrimitiveMappings =
    {
        new("string", "StringContextlessSchema", "StringContextSchema", "Z.String", false), new("int", "IntContextlessSchema", "IntContextSchema", "Z.Int", true),
        new("double", "DoubleContextlessSchema", "DoubleContextSchema", "Z.Double", true), new("decimal", "DecimalContextlessSchema", "DecimalContextSchema", "Z.Decimal", true),
        new("bool", "BoolContextlessSchema", "BoolContextSchema", "Z.Bool", true), new("System.Guid", "GuidContextlessSchema", "GuidContextSchema", "Z.Guid", true),
        new("System.DateTime", "DateTimeContextlessSchema", "DateTimeContextSchema", "Z.DateTime", true)
    };

    /// <summary>
    /// Types introduced in .NET 6 (DateOnly, TimeOnly).
    /// </summary>
    internal static readonly Mapping[] ModernNetMappings =
    {
        new("DateOnly", "DateOnlyContextlessSchema", "DateOnlyContextSchema", "Z.DateOnly", true), new("TimeOnly", "TimeOnlyContextlessSchema", "TimeOnlyContextSchema", "Z.TimeOnly", true)
    };

    internal static readonly string[] Collections =
    [
        "{0}[]",
        "System.Collections.Generic.List<{0}>",
        "System.Collections.Generic.ICollection<{0}>"
    ];
}