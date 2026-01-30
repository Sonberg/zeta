namespace Zeta.SourceGenerators;

/// <summary>
/// Defines mappings between C# types and their corresponding schema classes.
/// </summary>
internal static class SchemaMapping
{
    internal record Mapping(string Type, string SchemaClass, string FactoryMethod);

    /// <summary>
    /// Core primitive types that have schema implementations.
    /// </summary>
    internal static readonly Mapping[] PrimitiveMappings =
    {
        new("string", "StringContextlessSchema", "Z.String"),
        new("int", "IntContextlessSchema", "Z.Int"),
        new("double", "DoubleContextlessSchema", "Z.Double"),
        new("decimal", "DecimalContextlessSchema", "Z.Decimal"),
        new("bool", "BoolContextlessSchema", "Z.Bool"),
        new("System.Guid", "GuidContextlessSchema", "Z.Guid"),
        new("System.DateTime", "DateTimeContextlessSchema", "Z.DateTime")
    };
}
