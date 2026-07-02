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
    public async Task StatefulRefinementRule_Contextless_SyncAndAsync_Validate()
    {
        var syncRule = new StatefulRefinementRule<int, int>(
            static (value, ctx, min) => value >= min
                ? null
                : new ValidationError(ctx.PathSegments, "min_value", $"Must be >= {min}"), 10);

        var asyncRule = new StatefulRefinementRule<int, int>(
            static (value, ctx, min) => ValueTask.FromResult<ValidationError?>(value >= min
                ? null
                : new ValidationError(ctx.PathSegments, "min_value", $"Must be >= {min}")), 10);

        var context = new ValidationContext();
        Assert.Null(await syncRule.ValidateAsync(11, context));
        Assert.NotNull(await syncRule.ValidateAsync(5, context));
        Assert.Null(await asyncRule.ValidateAsync(12, context));
        Assert.NotNull(await asyncRule.ValidateAsync(2, context));
    }

    [Fact]
    public async Task StatefulRefinementRule_ContextAware_SyncAndAsync_Validate()
    {
        var syncRule = new StatefulRefinementRule<int, Ctx, int>(
            static (value, ctx, offset) => value <= ctx.Data.Limit + offset
                ? null
                : new ValidationError(ctx.PathSegments, "too_large", "Too large"), 0);

        var asyncRule = new StatefulRefinementRule<int, Ctx, int>(
            static (value, ctx, offset) => ValueTask.FromResult<ValidationError?>(value <= ctx.Data.Limit + offset
                ? null
                : new ValidationError(ctx.PathSegments, "too_large", "Too large")), 0);

        var context = new ValidationContext<Ctx>(new Ctx(10));
        Assert.Null(await syncRule.ValidateAsync(10, context));
        Assert.NotNull(await syncRule.ValidateAsync(11, context));
        Assert.Null(await asyncRule.ValidateAsync(9, context));
        Assert.NotNull(await asyncRule.ValidateAsync(50, context));
    }

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
