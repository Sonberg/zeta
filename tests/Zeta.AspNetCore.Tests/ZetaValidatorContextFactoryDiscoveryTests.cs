using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Zeta.AspNetCore.Tests;

public class ZetaValidatorContextFactoryDiscoveryTests
{
    private interface IAnimal
    {
        string Name { get; }
    }

    private sealed record Cat(string Name, int ClawSharpness) : IAnimal;
    private sealed record Dog(string Name, int BarkVolum) : IAnimal;
    private sealed record DogContext(bool Value);

    [Fact]
    public async Task ValidateAsync_ContextFactoryDefinedInNestedSchema_IsDiscovered()
    {
        var services = new ServiceCollection();
        services.AddScoped<IZetaValidator, ZetaValidator>();
        await using var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IZetaValidator>();

        var schema = Z.Object<Dog>()
            .Using<DogContext>()
            .If(_ => true, _ => _);

        SetFactoryOnConditionalSchema(
            schema,
            0,
            (_, _, _) => Task.FromResult(new DogContext(false)));

        var result = await validator.ValidateAsync<Dog, DogContext>(new Dog("Rex", 50), schema);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "dog_context");
    }

    [Fact]
    public async Task ValidateAsync_MultipleContextFactoriesInSchemaTree_Throws()
    {
        var services = new ServiceCollection();
        services.AddScoped<IZetaValidator, ZetaValidator>();
        await using var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IZetaValidator>();

        var schema = Z.Object<Dog>()
            .Using<DogContext>()
            .If(_ => true, _ => _)
            .If(_ => true, _ => _);

        SetFactoryOnConditionalSchema(
            schema,
            0,
            (_, _, _) => Task.FromResult(new DogContext(true)));
        SetFactoryOnConditionalSchema(
            schema,
            1,
            (_, _, _) => Task.FromResult(new DogContext(false)));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await validator.ValidateAsync<Dog, DogContext>(new Dog("Rex", 50), schema));

        Assert.Contains("Multiple context factories", ex.Message);
    }

    [Fact]
    public async Task ValidateAsync_IfDerivedContextOverload_PromotesRootSchema()
    {
        var services = new ServiceCollection();
        services.AddScoped<IZetaValidator, ZetaValidator>();
        await using var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IZetaValidator>();

        var schema = Z.Object<IAnimal>()
            .Field(x => x.Name, n => n.MinLength(3))
            .If<Dog, DogContext>(x => x
                .Field(d => d.BarkVolum, v => v.Min(0).Max(100))
                .Using<DogContext>((_, _, _) => Task.FromResult(new DogContext(false)))
                .Refine((_, ctx) => ctx.Value, "Dog context value must be true", "dog_context"))
            .If(x => x is Cat, x => x.As<Cat>().Field(c => c.ClawSharpness, v => v.Min(0).Max(100)));

        var result = await validator.ValidateAsync<IAnimal, DogContext>(new Dog("Rex", 50), schema);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "dog_context");
    }

    private static void SetFactoryOnConditionalSchema(
        object rootSchema,
        int conditionalIndex,
        Func<Dog, IServiceProvider, CancellationToken, Task<DogContext>> factory)
    {
        var conditionalsField = FindField(rootSchema.GetType(), "_conditionals")
            ?? throw new InvalidOperationException("Could not locate schema conditionals field.");

        if (conditionalsField.GetValue(rootSchema) is not System.Collections.IEnumerable conditionals)
        {
            throw new InvalidOperationException("Schema conditionals were not initialized.");
        }

        var conditional = conditionals.Cast<object>().ElementAt(conditionalIndex);
        var nestedSchemaField = FindField(conditional.GetType(), "_schema") 
            ?? FindField(conditional.GetType(), "Item2") // Handle ValueTuple in ContextlessSchema
            ?? throw new InvalidOperationException($"Could not locate nested conditional schema on type {conditional.GetType().Name}.");
            
        var nestedSchema = nestedSchemaField.GetValue(conditional)
            ?? throw new InvalidOperationException("Conditional schema is null.");

        // Penetrate adapters if necessary
        while (nestedSchema.GetType().Name.Contains("Adapter"))
        {
            var innerField = FindField(nestedSchema.GetType(), "_inner") ?? FindField(nestedSchema.GetType(), "_schema");
            if (innerField == null) break;
            nestedSchema = innerField.GetValue(nestedSchema) ?? nestedSchema;
        }

        var setFactoryMethod = nestedSchema.GetType().GetMethod("SetContextFactory", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Could not locate SetContextFactory method on type {nestedSchema.GetType().Name}.");
        setFactoryMethod.Invoke(nestedSchema, [factory]);
    }

    private static FieldInfo? FindField(Type type, string name)
    {
        while (type != null)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field != null) return field;
            type = type.BaseType!;
        }

        return null;
    }
}
