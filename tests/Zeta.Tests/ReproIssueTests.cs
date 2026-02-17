using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zeta.AspNetCore;
using Zeta.Core;
using Zeta.Schemas;

namespace Zeta.Tests;

public class ReproIssueTests
{
    private interface IAnimal
    {
        string Name { get; }
    }

    private record Dog(string Name, int BarkVolum) : IAnimal;
    private record Cat(string Name, int ClawSharpness) : IAnimal;
    private record CatContext(bool Value);

    [Fact]
    public async Task UserScenario_ShouldWork()
    {
        var services = new ServiceCollection();
        services.AddScoped<IZetaValidator, ZetaValidator>();
        await using var provider = services.BuildServiceProvider();

        var scope = provider.CreateScope();
        var zeta = scope.ServiceProvider.GetRequiredService<IZetaValidator>();

        var dogSchema = Z.Object<Dog>()
            .Using<CatContext>(async (_, _, _) => new CatContext(false))
            .Field(d => d.BarkVolum, v => v.Min(0).Max(100))
            .Refine((_, ctx) => ctx.Value, "Dog context value must be true");

        var catSchema = Z.Object<Cat>()
            .Field(c => c.ClawSharpness, v => v.Min(0).Max(100));

        // Schema stays contextless — context resolution happens via SelfResolvingSchema
        var schema = Z.Object<IAnimal>()
            .Field(x => x.Name, n => n.MinLength(3))
            .If(x => x is Dog, dogSchema)
            .If(x => x is Cat, catSchema);

        var result = await zeta.ValidateAsync(new Dog("Rex", 50), schema);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Message == "Dog context value must be true");
    }
}
