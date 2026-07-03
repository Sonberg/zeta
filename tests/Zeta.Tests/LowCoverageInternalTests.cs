using Microsoft.Extensions.DependencyInjection;
using Zeta.Adapters;
using Zeta.Rules;
using Zeta.Schemas;

namespace Zeta.Tests;

public class LowCoverageInternalTests
{
    private sealed record Ctx(int Limit);
    private interface IAnimal;
    private sealed record Dog(int WoofVolume) : IAnimal;
    private sealed record Cat(int ClawSharpness) : IAnimal;

    [Fact]
    public async Task TypeAssertion_InternalTypes_CoverMismatchAndFactories()
    {
        var contextlessDogSchema = Z.Schema<Dog>()
            .Property(x => x.WoofVolume, s => s.Min(0).Max(100));
        var contextlessAssertion = new ContextlessTypeAssertion<IAnimal, Dog>(contextlessDogSchema);

        var mismatch = await contextlessAssertion.ValidateAsync(new Cat(5), new ValidationContext());
        Assert.Single(mismatch);
        Assert.Equal("type_mismatch", mismatch[0].Code);

        var valid = await contextlessAssertion.ValidateAsync(new Dog(50), new ValidationContext());
        Assert.Empty(valid);

        var contextDogSchema = Z.Schema<Dog>()
            .Using<Ctx>((value, _, _) => ValueTask.FromResult(new Ctx(value.WoofVolume)))
            .Property(x => x.WoofVolume, s => s.Min(0).Max(100));
        var contextAwareAssertion = new ContextAwareTypeAssertion<IAnimal, Dog, Ctx>(contextDogSchema);

        var typedMismatch = await contextAwareAssertion.ValidateAsync(new Cat(1), new ValidationContext<Ctx>(new Ctx(0)));
        Assert.Single(typedMismatch);
        Assert.Equal("type_mismatch", typedMismatch[0].Code);

        var factories = contextAwareAssertion.GetContextFactories().ToList();
        Assert.NotEmpty(factories);
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await factories[0](new Cat(1), new ServiceCollection().BuildServiceProvider(), CancellationToken.None));
    }
}
