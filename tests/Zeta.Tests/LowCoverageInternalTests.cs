using Microsoft.Extensions.DependencyInjection;
using Zeta.Adapters;
using Zeta.Rules;
using Zeta.Schemas;
using Zeta.Validators;

namespace Zeta.Tests;

public class LowCoverageInternalTests
{
    private sealed record Ctx(int Limit);
    private interface IAnimal;
    private sealed record Dog(int WoofVolume) : IAnimal;
    private sealed record Cat(int ClawSharpness) : IAnimal;
    private sealed record Person(string? Name);

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
    public async Task RequiredFieldValidators_ReturnExpectedErrors()
    {
        var contextless = new RequiredFieldContextlessValidator<Person, string?>("Name", p => p.Name, null);
        var context = new ValidationContext();
        var missingErrors = await contextless.ValidateAsync(new Person(null), context);
        Assert.Single(missingErrors);
        Assert.Equal("$.name", missingErrors[0].PathString);
        Assert.Equal("required", missingErrors[0].Code);

        var withValueErrors = await contextless.ValidateAsync(new Person("ok"), context);
        Assert.Empty(withValueErrors);

        var contextAware = new RequiredFieldContextContextValidator<Person, string?, Ctx>("Name", p => p.Name, "Name is required");
        var typedContext = new ValidationContext<Ctx>(new Ctx(0));
        var missingCtxErrors = await contextAware.ValidateAsync(new Person(null), typedContext);
        Assert.Single(missingCtxErrors);
        Assert.Equal("$.name", missingCtxErrors[0].PathString);
        Assert.Equal("Name is required", missingCtxErrors[0].Message);
    }

    [Fact]
    public async Task NullableStructAdapters_HandleNullAndInvalidValues()
    {
        var inner = Z.Int().Min(5);

        var contextlessAdapter = new NullableStructContextlessAdapter<int>(inner);
        var contextlessNull = await contextlessAdapter.ValidateAsync(null, new ValidationContext());
        Assert.True(contextlessNull.IsFailure);
        Assert.Equal("null_value", contextlessNull.Errors[0].Code);

        var contextlessInvalid = await contextlessAdapter.ValidateAsync(1, new ValidationContext());
        Assert.True(contextlessInvalid.IsFailure);

        var contextAdapter = new NullableStructContextAdapter<int, Ctx>(inner);
        var typedContext = new ValidationContext<Ctx>(new Ctx(0));
        var contextNull = await contextAdapter.ValidateAsync(null, typedContext);
        Assert.True(contextNull.IsFailure);
        Assert.Equal("null_value", contextNull.Errors[0].Code);

        var contextValid = await contextAdapter.ValidateAsync(10, typedContext);
        Assert.True(contextValid.IsSuccess);
    }

    [Fact]
    public async Task NullableWrappers_HandleNullFactoriesAndValidation()
    {
        var intSchema = Z.Int()
            .Using<Ctx>((value, _, _) => ValueTask.FromResult(new Ctx(value)))
            .Min(5);
        var intWrapper = new NullableStructContextWrapper<int, Ctx>(intSchema);

        var intFactories = ((ISchema<int?, Ctx>)intWrapper).GetContextFactories().ToList();
        Assert.Single(intFactories);
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await intFactories[0](null, new ServiceCollection().BuildServiceProvider(), CancellationToken.None));

        var intTypedContext = new ValidationContext<Ctx>(new Ctx(0));
        var intNullResult = await intWrapper.ValidateAsync(null, intTypedContext);
        Assert.True(intNullResult.IsFailure);
        var intValidResult = await intWrapper.ValidateAsync(10, intTypedContext);
        Assert.True(intValidResult.IsSuccess);

        var stringSchema = Z.String()
            .Using<Ctx>((value, _, _) => ValueTask.FromResult(new Ctx(value.Length)))
            .MinLength(3);
        var stringWrapper = new NullableReferenceContextWrapper<string, Ctx>(stringSchema);

        var stringFactories = ((ISchema<string?, Ctx>)stringWrapper).GetContextFactories().ToList();
        Assert.Single(stringFactories);
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await stringFactories[0](null, new ServiceCollection().BuildServiceProvider(), CancellationToken.None));

        var stringTypedContext = new ValidationContext<Ctx>(new Ctx(0));
        var stringInvalid = await stringWrapper.ValidateAsync("a", stringTypedContext);
        Assert.True(stringInvalid.IsFailure);
        var stringValid = await stringWrapper.ValidateAsync("abcd", stringTypedContext);
        Assert.True(stringValid.IsSuccess);
    }

    [Fact]
    public async Task TypeAssertion_InternalTypes_CoverMismatchAndFactories()
    {
        var contextlessDogSchema = Z.Object<Dog>()
            .Field(x => x.WoofVolume, s => s.Min(0).Max(100));
        var contextlessAssertion = new ContextlessTypeAssertion<IAnimal, Dog>(contextlessDogSchema);

        var mismatch = await contextlessAssertion.ValidateAsync(new Cat(5), new ValidationContext());
        Assert.Single(mismatch);
        Assert.Equal("type_mismatch", mismatch[0].Code);

        var valid = await contextlessAssertion.ValidateAsync(new Dog(50), new ValidationContext());
        Assert.Empty(valid);

        var contextDogSchema = Z.Object<Dog>()
            .Using<Ctx>((value, _, _) => ValueTask.FromResult(new Ctx(value.WoofVolume)))
            .Field(x => x.WoofVolume, s => s.Min(0).Max(100));
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
