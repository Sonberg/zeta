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

        var dogSchema = Z.Object<Dog>()
            .Using<DogContext>((_, _, _) => ValueTask.FromResult(new DogContext(false)))
            .Field(d => d.BarkVolum, v => v.Min(0).Max(100))
            .Refine((_, ctx) => ctx.Value, "Dog context value must be true", "dog_context");

        var schema = Z.Object<IAnimal>()
            .Field(x => x.Name, n => n.MinLength(3))
            .If(x => x is Dog, dogSchema)
            .If(x => x is Cat, x => x.As<Cat>().Field(c => c.ClawSharpness, v => v.Min(0).Max(100)));

        var result = await validator.ValidateAsync(new Dog("Rex", 50), schema);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "dog_context");
    }

    [Fact]
    public async Task ValidateAsync_MultipleBranchFactories_UsesApplicableFactory()
    {
        var services = new ServiceCollection();
        services.AddScoped<IZetaValidator, ZetaValidator>();
        await using var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IZetaValidator>();

        var dogSchema = Z.Object<Dog>()
            .Using<DogContext>((_, _, _) => ValueTask.FromResult(new DogContext(true)))
            .Field(d => d.BarkVolum, v => v.Min(0).Max(100))
            .Refine((_, ctx) => ctx.Value, "dog branch context must be true", "dog_context");

        var catSchema = Z.Object<Cat>()
            .Using<DogContext>((_, _, _) => ValueTask.FromResult(new DogContext(false)))
            .Field(c => c.ClawSharpness, v => v.Min(0).Max(100));

        var schema = Z.Object<IAnimal>()
            .If(x => x is Dog, dogSchema)
            .If(x => x is Cat, catSchema);

        var result = await validator.ValidateAsync(new Dog("Rex", 50), schema);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidateAsync_RootAndBranchFactory_BothApplicable_Throws()
    {
        var services = new ServiceCollection();
        services.AddScoped<IZetaValidator, ZetaValidator>();
        await using var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IZetaValidator>();

        var dogBranchSchema = Z.Object<Dog>()
            .Using<DogContext>((_, _, _) => ValueTask.FromResult(new DogContext(true)))
            .Field(d => d.BarkVolum, v => v.Min(0).Max(100));

        var schema = Z.Object<IAnimal>()
            .Using<DogContext>((_, _, _) => ValueTask.FromResult(new DogContext(true)))
            .If(x => x is Dog, dogBranchSchema);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await validator.ValidateAsync<IAnimal, DogContext>(new Dog("Rex", 50), schema));

        Assert.Contains("Multiple applicable context factories", ex.Message);
    }

    [Fact]
    public async Task ValidateAsync_IfDerivedContextOverload_SelfResolves()
    {
        var services = new ServiceCollection();
        services.AddScoped<IZetaValidator, ZetaValidator>();
        await using var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IZetaValidator>();

        var dogSchema = Z.Object<Dog>()
            .Field(d => d.BarkVolum, v => v.Min(0).Max(100))
            .Using<DogContext>((_, _, _) => ValueTask.FromResult(new DogContext(false)))
            .Refine((_, ctx) => ctx.Value, "Dog context value must be true", "dog_context");

        var catSchema = Z.Object<Cat>()
            .Field(c => c.ClawSharpness, v => v.Min(0).Max(100));

        var schema = Z.Object<IAnimal>()
            .Field(x => x.Name, n => n.MinLength(3))
            .If(x => x is Dog, dogSchema)
            .If(x => x is Cat, catSchema);

        var result = await validator.ValidateAsync(new Dog("Rex", 50), schema);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "dog_context");
    }

}
