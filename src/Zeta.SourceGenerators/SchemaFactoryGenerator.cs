using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Zeta.SourceGenerators;

/// <summary>
/// Main source generator that orchestrates generation of schema-related code.
/// </summary>
[Generator]
public class SchemaFactoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            // Generate ObjectContextlessSchema Field overloads
            var objectSchemaCode = ObjectSchemaFieldGenerator.Generate();
            ctx.AddSource("ObjectContextlessSchema.g.cs", SourceText.From(objectSchemaCode, Encoding.UTF8));

            // Generate ObjectContextSchema Field overloads
            var objectSchemaWithContextCode = ObjectContextSchemaFieldGenerator.Generate();
            ctx.AddSource("ObjectContextSchema.g.cs", SourceText.From(objectSchemaWithContextCode, Encoding.UTF8));

            // Generate CollectionSchemaExtensions
            var collectionExtensionsCode = CollectionExtensionsGenerator.Generate();
            ctx.AddSource("CollectionSchemaExtensions.g.cs", SourceText.From(collectionExtensionsCode, Encoding.UTF8));

            // Generate DictionarySchemaExtensions
            var dictionaryExtensionsCode = DictionaryExtensionsGenerator.Generate();
            ctx.AddSource("DictionarySchemaExtensions.g.cs", SourceText.From(dictionaryExtensionsCode, Encoding.UTF8));
        });
    }
}
